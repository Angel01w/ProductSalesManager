<!-- src/components/SaleItem.vue -->
<template>
    <div class="container py-4">
        <!-- Top bar -->
        <div class="d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-2 mb-3">
            <div>
                <h4 class="mb-1">Detalle de Ventas (SaleItems)</h4>
                <div class="text-muted small">CRUD (API: /api/saleitems)</div>
            </div>

            <div class="d-flex gap-2">
                <div class="input-group">
                    <span class="input-group-text">🔎</span>
                    <input v-model.trim="search" class="form-control" placeholder="Buscar por producto o SaleId..." />
                </div>
                <button class="btn btn-outline-secondary" @click="load" :disabled="loading">
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
                                <h6 class="mb-1">{{ isEdit ? "Editar item" : "Nuevo item" }}</h6>
                                <div class="text-muted small">
                                    {{ isEdit ? `ID #${form.id}` : "Asocia un producto a una venta" }}
                                </div>
                            </div>
                            <span class="badge" :class="isEdit ? 'text-bg-warning' : 'text-bg-info'">
                                {{ isEdit ? "EDIT" : "CREATE" }}
                            </span>
                        </div>

                        <form @submit.prevent="onSubmit" novalidate>
                            <!-- Sale -->
                            <div class="mb-3">
                                <label class="form-label">Venta (SaleId)</label>
                                <select v-model.number="form.saleId" class="form-select" :class="{ 'is-invalid': !!errs.saleId }">
                                    <option :value="0" disabled>-- Seleccionar --</option>
                                    <option v-for="s in sales" :key="s.id" :value="s.id">
                                        Venta #{{ s.id }} — {{ s.customer?.name || "Sin cliente" }} — {{ money(s.total) }}
                                    </option>
                                </select>
                                <div class="invalid-feedback">{{ errs.saleId }}</div>
                                <div class="text-muted small mt-1">
                                    * Debe existir en el backend, si no devuelve: "La venta (SaleId) no existe."
                                </div>
                            </div>

                            <!-- Product -->
                            <div class="mb-3">
                                <label class="form-label">Producto</label>
                                <select v-model.number="form.productId" class="form-select" :class="{ 'is-invalid': !!errs.productId }">
                                    <option :value="0" disabled>-- Seleccionar --</option>
                                    <option v-for="p in products" :key="p.id" :value="p.id">
                                        {{ p.name }} — {{ money(p.price) }}
                                    </option>
                                </select>
                                <div class="invalid-feedback">{{ errs.productId }}</div>
                            </div>

                            <div class="row g-2">
                                <div class="col-6">
                                    <label class="form-label">Cantidad</label>
                                    <input v-model.number="form.quantity"
                                           type="number"
                                           min="1"
                                           class="form-control"
                                           :class="{ 'is-invalid': !!errs.quantity }" />
                                    <div class="invalid-feedback">{{ errs.quantity }}</div>
                                </div>

                                <div class="col-6">
                                    <label class="form-label">Precio Unitario</label>
                                    <input v-model.number="form.unitPrice"
                                           type="number"
                                           step="0.01"
                                           min="0"
                                           class="form-control"
                                           :class="{ 'is-invalid': !!errs.unitPrice }" />
                                    <div class="invalid-feedback">{{ errs.unitPrice }}</div>
                                </div>
                            </div>

                            <!-- Summary -->
                            <div class="rounded-3 p-3 bg-light border mt-3 mb-3">
                                <div class="d-flex justify-content-between">
                                    <span class="text-muted small">Total línea</span>
                                    <span class="fw-semibold">{{ money(lineTotal) }}</span>
                                </div>
                                <div class="text-muted small mt-1">LineTotal = Quantity × UnitPrice</div>
                            </div>

                            <div class="d-flex gap-2">
                                <button class="btn btn-info text-white" type="submit" :disabled="saving">
                                    <span v-if="saving" class="spinner-border spinner-border-sm me-2"></span>
                                    {{ isEdit ? "Actualizar" : "Crear" }}
                                </button>

                                <button v-if="isEdit"
                                        class="btn btn-outline-secondary"
                                        type="button"
                                        @click="resetForm"
                                        :disabled="saving">
                                    Cancelar
                                </button>
                            </div>
                        </form>

                        <hr class="my-3" />

                        <div class="text-muted small">
                            <div class="d-flex justify-content-between">
                                <span>API Base</span>
                                <span class="fw-semibold">{{ API_BASE }}</span>
                            </div>
                            <div class="d-flex justify-content-between">
                                <span>Endpoint</span>
                                <span class="fw-semibold">/api/saleitems</span>
                            </div>
                            <div class="d-flex justify-content-between">
                                <span>Registros</span>
                                <span class="fw-semibold">{{ items.length }}</span>
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
                                <h6 class="mb-1">Listado</h6>
                                <div class="text-muted small">Mostrando: {{ filtered.length }}</div>
                            </div>

                            <div class="d-flex gap-2">
                                <button class="btn btn-outline-dark" @click="resetForm" :disabled="saving">Limpiar formulario</button>
                            </div>
                        </div>

                        <div class="table-responsive">
                            <table class="table table-hover align-middle">
                                <thead class="table-light">
                                    <tr>
                                        <th style="width: 80px">ID</th>
                                        <th style="width: 90px">SaleId</th>
                                        <th>Producto</th>
                                        <th class="text-end" style="width: 120px">Cantidad</th>
                                        <th class="text-end" style="width: 140px">Precio</th>
                                        <th class="text-end" style="width: 140px">Total</th>
                                        <th class="text-end" style="width: 190px">Acciones</th>
                                    </tr>
                                </thead>

                                <tbody>
                                    <tr v-if="loading">
                                        <td colspan="7" class="py-4 text-center">
                                            <span class="spinner-border spinner-border-sm me-2"></span>
                                            Cargando...
                                        </td>
                                    </tr>

                                    <tr v-else-if="filtered.length === 0">
                                        <td colspan="7" class="py-4 text-center text-muted">
                                            No hay items. Crea uno usando el formulario.
                                        </td>
                                    </tr>

                                    <tr v-else v-for="it in filtered" :key="it.id">
                                        <td class="fw-semibold">{{ it.id }}</td>
                                        <td class="fw-semibold">{{ it.saleId }}</td>

                                        <td>
                                            <div class="d-flex align-items-center gap-2">
                                                <span class="avatar">{{ initials(productLabel(it)) }}</span>
                                                <div class="min-w-0">
                                                    <div class="fw-semibold text-truncate">{{ productLabel(it) }}</div>
                                                    <div class="text-muted small">ProductId: {{ it.productId }}</div>
                                                </div>
                                            </div>
                                        </td>

                                        <td class="text-end">{{ it.quantity }}</td>
                                        <td class="text-end">{{ money(it.unitPrice) }}</td>
                                        <td class="text-end fw-semibold">{{ money(it.lineTotal ?? (Number(it.quantity) * Number(it.unitPrice))) }}</td>

                                        <td class="text-end">
                                            <button class="btn btn-sm btn-outline-primary me-2" @click="edit(it)" :disabled="saving">
                                                Editar
                                            </button>
                                            <button class="btn btn-sm btn-outline-danger" @click="remove(it)" :disabled="saving">
                                                Eliminar
                                            </button>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>

                        <div class="text-muted small mt-2">
                            * El backend valida SaleId, ProductId, Quantity &gt; 0 y UnitPrice &gt;= 0.
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Mini footer -->
        <div class="text-center text-muted small mt-3">© {{ new Date().getFullYear() }} ProductSalesManager</div>
    </div>
</template>

<script setup>
    import { computed, onMounted, reactive, ref, watch } from "vue";
    import axios from "axios";

    const API_BASE = import.meta.env.VITE_API_BASE_URL || "https://localhost:7276";

    const api = axios.create({
        baseURL: API_BASE,
        headers: { "Content-Type": "application/json" },
    });

    const loading = ref(false);
    const saving = ref(false);
    const error = ref("");
    const success = ref("");
    const search = ref("");

    const items = ref([]);
    const sales = ref([]);
    const products = ref([]);

    const form = reactive({
        id: null,
        saleId: 0,
        productId: 0,
        quantity: 1,
        unitPrice: 0,
    });

    const errs = reactive({
        saleId: "",
        productId: "",
        quantity: "",
        unitPrice: "",
    });

    const isEdit = computed(() => !!form.id);
    const lineTotal = computed(() => Number(form.quantity || 0) * Number(form.unitPrice || 0));

    const filtered = computed(() => {
        const s = search.value.toLowerCase();
        if (!s) return items.value;

        return items.value.filter((x) => {
            const bySale = String(x.saleId ?? "").includes(s);
            const byProduct =
                (x.productName || x.product?.name || productByIdName(x.productId) || "").toLowerCase().includes(s);
            return bySale || byProduct;
        });
    });

    function clearMsgs() {
        error.value = "";
        success.value = "";
    }

    function normalizeApiError(e, fallback) {
        // axios: cuando es "Network Error" no hay response
        return e?.response?.data?.message || e?.message || fallback;
    }

    function initials(name) {
        const parts = (name || "").trim().split(/\s+/).filter(Boolean);
        if (parts.length === 0) return "I";
        const first = parts[0]?.[0] ?? "";
        const last = parts.length > 1 ? parts[parts.length - 1]?.[0] ?? "" : "";
        return (first + last).toUpperCase();
    }

    function money(value) {
        const n = Number(value ?? 0);
        return n.toLocaleString(undefined, { style: "currency", currency: "USD" });
    }

    function productByIdName(productId) {
        const p = products.value.find((x) => x.id === productId);
        return p?.name || "";
    }

    function productLabel(it) {
        return it.productName || it.product?.name || productByIdName(it.productId) || "Producto";
    }

    function resetForm() {
        form.id = null;
        form.saleId = 0;
        form.productId = 0;
        form.quantity = 1;
        form.unitPrice = 0;

        errs.saleId = "";
        errs.productId = "";
        errs.quantity = "";
        errs.unitPrice = "";
    }

    function validate() {
        errs.saleId = "";
        errs.productId = "";
        errs.quantity = "";
        errs.unitPrice = "";

        if (!(form.saleId > 0)) errs.saleId = "SaleId inválido.";
        if (!(form.productId > 0)) errs.productId = "ProductId inválido.";
        if (!(Number(form.quantity) > 0)) errs.quantity = "Quantity debe ser > 0.";
        if (Number(form.unitPrice) < 0) errs.unitPrice = "UnitPrice no puede ser negativo.";

        return !errs.saleId && !errs.productId && !errs.quantity && !errs.unitPrice;
    }

    // Autollenar unitPrice cuando seleccionas producto (solo en CREATE, o si unitPrice está 0)
    watch(
        () => form.productId,
        (pid) => {
            const p = products.value.find((x) => x.id === pid);
            if (!p) return;
            if (!isEdit.value && (form.unitPrice === 0 || form.unitPrice === null)) {
                form.unitPrice = Number(p.price ?? 0);
            }
        }
    );

    async function load() {
        clearMsgs();
        loading.value = true;
        try {
            const [itRes, sRes, pRes] = await Promise.all([
                api.get("/api/saleitems"),
                api.get("/api/sales"),
                api.get("/api/products"),
            ]);

            items.value = itRes.data ?? [];
            sales.value = sRes.data ?? [];
            products.value = pRes.data ?? [];
        } catch (e) {
            error.value = normalizeApiError(e, "Error cargando datos.");
        } finally {
            loading.value = false;
        }
    }

    function edit(it) {
        clearMsgs();
        form.id = it.id;
        form.saleId = Number(it.saleId ?? 0);
        form.productId = Number(it.productId ?? 0);
        form.quantity = Number(it.quantity ?? 1);
        form.unitPrice = Number(it.unitPrice ?? 0);
    }

    async function onSubmit() {
        clearMsgs();
        if (!validate()) return;

        saving.value = true;
        try {
            const payload = {
                saleId: Number(form.saleId),
                productId: Number(form.productId),
                quantity: Number(form.quantity),
                unitPrice: Number(form.unitPrice),
            };

            if (!isEdit.value) {
                await api.post("/api/saleitems", payload);
                success.value = "Detalle creado.";
            } else {
                await api.put(`/api/saleitems/${form.id}`, payload);
                success.value = "Detalle actualizado.";
            }

            resetForm();
            await load();
        } catch (e) {
            error.value = normalizeApiError(e, "Error guardando detalle.");
        } finally {
            saving.value = false;
        }
    }

    async function remove(it) {
        clearMsgs();
        const ok = confirm(`¿Eliminar el item #${it.id}?`);
        if (!ok) return;

        saving.value = true;
        try {
            await api.delete(`/api/saleitems/${it.id}`);
            success.value = "Detalle eliminado.";
            await load();
        } catch (e) {
            error.value = normalizeApiError(e, "Error eliminando detalle.");
        } finally {
            saving.value = false;
        }
    }

    onMounted(load);
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
        background: rgba(13, 202, 240, 0.18);
        color: rgb(13, 202, 240);
        flex: 0 0 auto;
    }

    .min-w-0 {
        min-width: 0;
    }
</style>
