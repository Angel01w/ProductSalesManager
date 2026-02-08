<!-- src/components/Sale.vue -->
<template>
    <div class="container py-4">
        <!-- Top bar -->
        <div class="d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-2 mb-3">
            <div>
                <h4 class="mb-1">Ventas</h4>
                <div class="text-muted small">Registrar • Editar • Eliminar (API: /api/sales)</div>
            </div>

            <div class="d-flex gap-2">
                <div class="input-group">
                    <span class="input-group-text">🔎</span>
                    <input v-model.trim="search" class="form-control" placeholder="Buscar por cliente o ID..." />
                </div>
                <button class="btn btn-outline-secondary" @click="loadAll" :disabled="loading">
                    <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
                    Refrescar
                </button>
            </div>
        </div>

        <!-- Alerts -->
        <div v-if="error" class="alert alert-danger d-flex align-items-center gap-2" role="alert">
            <span>⚠️</span>
            <div class="flex-grow-1">{{ error }}</div>
            <button type="button" class="btn-close" aria-label="Close" @click="error = ''"></button>
        </div>

        <div v-if="success" class="alert alert-success d-flex align-items-center gap-2" role="alert">
            <span>✅</span>
            <div class="flex-grow-1">{{ success }}</div>
            <button type="button" class="btn-close" aria-label="Close" @click="success = ''"></button>
        </div>

        <div class="row g-3">
            <!-- Form -->
            <div class="col-12 col-lg-4">
                <div class="card shadow-sm border-0">
                    <div class="card-body">
                        <div class="d-flex align-items-start justify-content-between mb-3">
                            <div>
                                <h6 class="mb-1">{{ isEdit ? "Editar venta" : "Nueva venta" }}</h6>
                                <div class="text-muted small">
                                    {{ isEdit ? `ID #${form.id}` : "Selecciona cliente y productos" }}
                                </div>
                            </div>
                            <span class="badge" :class="isEdit ? 'text-bg-warning' : 'text-bg-success'">
                                {{ isEdit ? "EDIT" : "CREATE" }}
                            </span>
                        </div>

                        <!-- Customer -->
                        <div class="mb-3">
                            <label class="form-label">Cliente</label>
                            <select v-model.number="form.customerId"
                                    class="form-select"
                                    :class="{ 'is-invalid': !!errs.customerId }">
                                <option :value="0" disabled>-- Seleccionar --</option>
                                <option v-for="c in customers" :key="c.id" :value="c.id">
                                    {{ c.name }} ({{ c.email }})
                                </option>
                            </select>
                            <div class="invalid-feedback">{{ errs.customerId }}</div>
                        </div>

                        <!-- Date -->
                        <div class="mb-3">
                            <label class="form-label">Fecha (opcional)</label>
                            <input v-model="form.date"
                                   type="datetime-local"
                                   class="form-control"
                                   :class="{ 'is-invalid': !!errs.date }" />
                            <div class="invalid-feedback">{{ errs.date }}</div>
                            <div class="text-muted small mt-1">
                                Si la dejas vacía, el backend usa <span class="fw-semibold">DateTime.UtcNow</span>.
                            </div>
                        </div>

                        <!-- Items -->
                        <div class="mb-2 d-flex align-items-center justify-content-between">
                            <label class="form-label m-0">Productos</label>
                            <span class="badge text-bg-light border">Items: {{ form.items.length }}</span>
                        </div>

                        <div v-if="form.items.length === 0" class="alert alert-light border mb-3">
                            Agrega al menos 1 producto.
                        </div>

                        <div v-for="(it, idx) in form.items" :key="idx" class="border rounded-3 p-2 mb-2">
                            <div class="d-flex align-items-center justify-content-between mb-2">
                                <div class="fw-semibold small">Item #{{ idx + 1 }}</div>
                                <button class="btn btn-sm btn-outline-danger"
                                        type="button"
                                        @click="removeItem(idx)"
                                        :disabled="saving">
                                    Quitar
                                </button>
                            </div>

                            <div class="mb-2">
                                <label class="form-label small">Producto</label>
                                <select v-model.number="it.productId"
                                        class="form-select"
                                        :class="{ 'is-invalid': itemError(idx, 'productId') }">
                                    <option :value="0" disabled>-- Seleccionar --</option>
                                    <option v-for="p in products" :key="p.id" :value="p.id">
                                        {{ p.name }} — {{ money(p.price) }} (Stock: {{ p.stock }})
                                    </option>
                                </select>
                                <div class="invalid-feedback">Producto requerido.</div>
                            </div>

                            <div>
                                <label class="form-label small">Cantidad</label>
                                <input v-model.number="it.quantity"
                                       type="number"
                                       min="1"
                                       class="form-control"
                                       :class="{ 'is-invalid': itemError(idx, 'quantity') }" />
                                <div class="invalid-feedback">Cantidad debe ser mayor a 0.</div>
                            </div>
                        </div>

                        <button class="btn btn-outline-success w-100 mb-3" type="button" @click="addItem" :disabled="saving">
                            + Agregar producto
                        </button>

                        <!-- Summary -->
                        <div class="rounded-3 p-3 bg-light border mb-3">
                            <div class="d-flex justify-content-between">
                                <span class="text-muted small">Subtotal</span>
                                <span class="fw-semibold">{{ money(calcTotal) }}</span>
                            </div>
                            <div class="text-muted small mt-1">
                                * El total final lo calcula el backend usando el precio actual del producto.
                            </div>
                        </div>

                        <!-- Actions -->
                        <div class="d-flex gap-2">
                            <button class="btn btn-success" type="button" @click="onSubmit" :disabled="saving">
                                <span v-if="saving" class="spinner-border spinner-border-sm me-2"></span>
                                {{ isEdit ? "Actualizar" : "Registrar" }}
                            </button>

                            <button v-if="isEdit" class="btn btn-outline-secondary" type="button" @click="resetForm" :disabled="saving">
                                Cancelar
                            </button>
                        </div>

                        <hr class="my-3" />

                        <div class="text-muted small">
                            <div class="d-flex justify-content-between">
                                <span>Endpoint</span>
                                <span class="fw-semibold">/api/sales</span>
                            </div>
                            <div class="d-flex justify-content-between">
                                <span>Ventas</span>
                                <span class="fw-semibold">{{ sales.length }}</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Table -->
            <div class="col-12 col-lg-8">
                <div class="card shadow-sm border-0">
                    <div class="card-body">
                        <div class="d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-2 mb-2">
                            <div>
                                <h6 class="mb-1">Historial</h6>
                                <div class="text-muted small">Mostrando: {{ filtered.length }}</div>
                            </div>
                            <div class="d-flex gap-2">
                                <button class="btn btn-outline-dark" @click="resetForm" :disabled="saving">
                                    Limpiar formulario
                                </button>
                            </div>
                        </div>

                        <div class="table-responsive">
                            <table class="table table-hover align-middle">
                                <thead class="table-light">
                                    <tr>
                                        <th style="width: 80px">ID</th>
                                        <th>Cliente</th>
                                        <th style="width: 180px">Fecha</th>
                                        <th class="text-end" style="width: 140px">Total</th>
                                        <th class="text-end" style="width: 210px">Acciones</th>
                                    </tr>
                                </thead>

                                <tbody>
                                    <tr v-if="loading">
                                        <td colspan="5" class="py-4 text-center">
                                            <span class="spinner-border spinner-border-sm me-2"></span>
                                            Cargando...
                                        </td>
                                    </tr>

                                    <tr v-else-if="filtered.length === 0">
                                        <td colspan="5" class="py-4 text-center text-muted">
                                            No hay ventas. Registra una usando el formulario.
                                        </td>
                                    </tr>

                                    <!-- ✅ key SOLO EN EL TEMPLATE -->
                                    <template v-else v-for="s in filtered" :key="'row-' + s.id">
                                        <!-- Row -->
                                        <tr>
                                            <td class="fw-semibold">{{ s.id }}</td>

                                            <td>
                                                <div class="d-flex align-items-center gap-2">
                                                    <span class="avatar">{{ initials(s.customer?.name) }}</span>
                                                    <div class="min-w-0">
                                                        <div class="fw-semibold text-truncate">{{ s.customer?.name || "-" }}</div>
                                                        <div class="text-muted small text-truncate">{{ s.customer?.email || "-" }}</div>
                                                    </div>
                                                </div>
                                            </td>

                                            <td>{{ fmtDate(s.date) }}</td>
                                            <td class="text-end fw-semibold">{{ money(s.total) }}</td>

                                            <td class="text-end">
                                                <button class="btn btn-sm btn-outline-secondary me-2" @click="toggleDetails(s.id)">
                                                    {{ expandedId === s.id ? "Ocultar" : "Detalles" }}
                                                </button>
                                                <button class="btn btn-sm btn-outline-primary me-2" @click="editSale(s)" :disabled="saving">
                                                    Editar
                                                </button>
                                                <button class="btn btn-sm btn-outline-danger" @click="removeSale(s)" :disabled="saving">
                                                    Eliminar
                                                </button>
                                            </td>
                                        </tr>

                                        <!-- Details (sin key) -->
                                        <tr v-if="expandedId === s.id" class="bg-light">
                                            <td colspan="5" class="p-0">
                                                <div class="p-3 border-top">
                                                    <div class="fw-semibold mb-2">Items</div>

                                                    <div class="table-responsive">
                                                        <table class="table table-sm mb-0">
                                                            <thead>
                                                                <tr>
                                                                    <th>Producto</th>
                                                                    <th class="text-end">Cantidad</th>
                                                                    <th class="text-end">Precio</th>
                                                                    <th class="text-end">Total Línea</th>
                                                                </tr>
                                                            </thead>
                                                            <tbody>
                                                                <tr v-for="it in (s.items || [])" :key="it.id ?? (it.productId + '-' + it.quantity)">
                                                                    <td>{{ it.productName }}</td>
                                                                    <td class="text-end">{{ it.quantity }}</td>
                                                                    <td class="text-end">{{ money(it.unitPrice) }}</td>
                                                                    <td class="text-end fw-semibold">{{ money(it.lineTotal) }}</td>
                                                                </tr>

                                                                <tr v-if="!s.items || s.items.length === 0">
                                                                    <td colspan="4" class="text-muted text-center">Sin items</td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>
                                    </template>
                                </tbody>
                            </table>
                        </div>

                        <div class="text-muted small mt-2">
                            * El backend valida stock y calcula el total.
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="text-center text-muted small mt-3">
            © {{ new Date().getFullYear() }} ProductSalesManager
        </div>
    </div>
</template>

<script setup>
    import { computed, onMounted, reactive, ref } from "vue";
    import axios from "axios";

    const API_BASE = "https://localhost:7276";

    const api = axios.create({
        baseURL: API_BASE,
        headers: { "Content-Type": "application/json" },
    });

    const loading = ref(false);
    const saving = ref(false);
    const error = ref("");
    const success = ref("");
    const search = ref("");

    const customers = ref([]);
    const products = ref([]);
    const sales = ref([]);

    const expandedId = ref(null);

    const form = reactive({
        id: null,
        customerId: 0,
        date: "", // datetime-local
        items: [],
    });

    const errs = reactive({
        customerId: "",
        date: "",
        items: "",
    });

    const isEdit = computed(() => !!form.id);

    const filtered = computed(() => {
        const s = search.value.toLowerCase();
        if (!s) return sales.value;

        return sales.value.filter((x) => {
            const byId = String(x.id ?? "").includes(s);
            const byCustomer = String(x.customer?.name ?? "").toLowerCase().includes(s);
            return byId || byCustomer;
        });
    });

    const calcTotal = computed(() => {
        let total = 0;
        for (const it of form.items) {
            const p = products.value.find((x) => x.id === it.productId);
            if (!p) continue;
            const q = Number(it.quantity || 0);
            total += q * Number(p.price || 0);
        }
        return total;
    });

    function clearMsgs() {
        error.value = "";
        success.value = "";
    }

    function normalizeApiError(e, fallback) {
        // muestra algo útil aunque el backend mande texto plano
        return e?.response?.data?.message || e?.response?.data || e?.message || fallback;
    }

    function initials(name) {
        const parts = String(name || "").trim().split(/\s+/).filter(Boolean);
        if (parts.length === 0) return "V";
        const first = parts[0]?.[0] ?? "";
        const last = parts.length > 1 ? parts[parts.length - 1]?.[0] ?? "" : "";
        return (first + last).toUpperCase();
    }

    function money(value) {
        const n = Number(value ?? 0);
        return n.toLocaleString(undefined, { style: "currency", currency: "USD" });
    }

    function fmtDate(iso) {
        if (!iso) return "-";
        const d = new Date(iso);
        if (Number.isNaN(d.getTime())) return String(iso);
        return d.toLocaleString();
    }

    function toIsoFromDatetimeLocal(v) {
        if (!v) return null;
        const d = new Date(v);
        if (Number.isNaN(d.getTime())) return null;
        return d.toISOString();
    }

    function resetForm() {
        form.id = null;
        form.customerId = 0;
        form.date = "";
        form.items = [];
        errs.customerId = "";
        errs.date = "";
        errs.items = "";
        expandedId.value = null;
        if (form.items.length === 0) addItem();
    }

    function addItem() {
        form.items.push({ productId: 0, quantity: 1 });
    }

    function removeItem(idx) {
        form.items.splice(idx, 1);
    }

    function itemError(idx, field) {
        const it = form.items[idx];
        if (!it) return false;
        if (field === "productId") return !(it.productId > 0);
        if (field === "quantity") return !(Number(it.quantity) > 0);
        return false;
    }

    function validate() {
        errs.customerId = "";
        errs.date = "";
        errs.items = "";

        if (!(form.customerId > 0)) errs.customerId = "Debes seleccionar un cliente.";
        if (!form.items || form.items.length === 0) errs.items = "Debes agregar al menos 1 producto.";

        if (form.items?.some((i) => !(i.productId > 0) || !(Number(i.quantity) > 0))) {
            errs.items = "Cada item debe tener ProductId y Quantity válidos.";
        }

        if (form.date) {
            const iso = toIsoFromDatetimeLocal(form.date);
            if (!iso) errs.date = "Fecha inválida.";
        }

        return !errs.customerId && !errs.date && !errs.items;
    }

    async function loadAll() {
        clearMsgs();
        loading.value = true;
        try {
            const [cRes, pRes, sRes] = await Promise.all([
                api.get("/api/customer"),
                api.get("/api/products"),
                api.get("/api/sales"),
            ]);

            customers.value = cRes.data ?? [];
            products.value = pRes.data ?? [];
            sales.value = sRes.data ?? [];
        } catch (e) {
            error.value = normalizeApiError(e, "Error cargando datos.");
        } finally {
            loading.value = false;
        }
    }

    function toggleDetails(id) {
        expandedId.value = expandedId.value === id ? null : id;
    }

    function editSale(s) {
        clearMsgs();
        form.id = s.id;
        form.customerId = s.customer?.id ?? s.customerId ?? 0;

        if (s.date) {
            const d = new Date(s.date);
            if (!Number.isNaN(d.getTime())) {
                const pad = (n) => String(n).padStart(2, "0");
                form.date = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(
                    d.getMinutes()
                )}`;
            } else {
                form.date = "";
            }
        } else {
            form.date = "";
        }

        form.items = (s.items || []).map((i) => ({
            productId: i.productId,
            quantity: i.quantity,
        }));

        if (form.items.length === 0) addItem();
        expandedId.value = s.id;
    }

    async function onSubmit() {
        clearMsgs();
        if (!validate()) {
            if (errs.items) error.value = errs.items;
            return;
        }

        saving.value = true;
        try {
            const payload = {
                customerId: form.customerId,
                date: form.date ? toIsoFromDatetimeLocal(form.date) : null,
                items: form.items.map((i) => ({ productId: i.productId, quantity: Number(i.quantity) })),
            };

            if (!isEdit.value) {
                await api.post("/api/sales", payload);
                success.value = "Venta registrada.";
            } else {
                await api.put(`/api/sales/${form.id}`, payload);
                success.value = "Venta actualizada.";
            }

            resetForm();
            await loadAll();
        } catch (e) {
            error.value = normalizeApiError(e, "Error guardando venta.");
        } finally {
            saving.value = false;
        }
    }

    async function removeSale(s) {
        clearMsgs();
        const ok = confirm(`¿Eliminar la venta #${s.id}? (El backend devolverá stock)`);
        if (!ok) return;

        saving.value = true;
        try {
            await api.delete(`/api/sales/${s.id}`);
            success.value = "Venta eliminada.";
            if (expandedId.value === s.id) expandedId.value = null;
            await loadAll();
        } catch (e) {
            error.value = normalizeApiError(e, "Error eliminando venta.");
        } finally {
            saving.value = false;
        }
    }

    onMounted(() => {
        if (form.items.length === 0) addItem();
        loadAll();
    });
</script>

<style scoped>
    .avatar {
        width: 40px;
        height: 40px;
        border-radius: 14px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        font-weight: 800;
        background: rgba(25, 135, 84, 0.14);
        color: rgb(25, 135, 84);
        flex: 0 0 auto;
    }

    .min-w-0 {
        min-width: 0;
    }
</style>
