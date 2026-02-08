<!-- src/components/SaleItem.vue -->
<template>
    <div class="container py-4">
        <!-- Top bar -->
        <div class="d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-2 mb-3">
            <div>
                <h4 class="mb-1">Detalle de Ventas (SaleItems)</h4>
                <div class="text-muted small">Listado (API: /api/saleitems)</div>
            </div>

            <div class="d-flex gap-2">
                <div class="input-group">
                    <span class="input-group-text">🔎</span>
                    <input v-model.trim="search"
                           class="form-control"
                           placeholder="Buscar por producto o SaleId..." />
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

        <!-- ONLY LIST -->
        <div class="card shadow-sm border-0">
            <div class="card-body">
                <div class="d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-2 mb-2">
                    <div>
                        <h6 class="mb-1">Listado</h6>
                        <div class="text-muted small">Mostrando: {{ filtered.length }}</div>
                    </div>

                    <button class="btn btn-outline-dark" @click="search = ''" :disabled="loading">
                        Limpiar búsqueda
                    </button>
                </div>

                <div class="table-responsive">
                    <table class="table table-hover align-middle">
                        <thead class="table-light">
                            <tr>
                                <th style="width: 90px">ID</th>
                                <th style="width: 100px">SaleId</th>
                                <th>Producto</th>
                                <th class="text-end" style="width: 120px">Cantidad</th>
                                <th class="text-end" style="width: 140px">Precio</th>
                                <th class="text-end" style="width: 140px">Total</th>
                                <th class="text-end" style="width: 140px">Acciones</th>
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
                                    No hay items en el historial.
                                </td>
                            </tr>

                            <tr v-else v-for="it in filtered" :key="it.id">
                                <td class="fw-semibold">{{ it.id }}</td>
                                <td class="fw-semibold">{{ it.saleId }}</td>

                                <td>
                                    <div class="d-flex align-items-center gap-2">
                                        <span class="avatar">{{ initials(it.productName) }}</span>
                                        <div class="min-w-0">
                                            <div class="fw-semibold text-truncate">{{ it.productName || "Sin nombre" }}</div>
                                            <div class="text-muted small">ProductId: {{ it.productId }}</div>
                                        </div>
                                    </div>
                                </td>

                                <td class="text-end">{{ it.quantity }}</td>
                                <td class="text-end">{{ money(it.unitPrice) }}</td>
                                <td class="text-end fw-semibold">{{ money(it.lineTotal) }}</td>

                                <td class="text-end">
                                    <button class="btn btn-sm btn-outline-danger"
                                            @click="remove(it)"
                                            :disabled="saving">
                                        Eliminar
                                    </button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <div class="text-muted small mt-2">
                    * Los items se generan automáticamente desde Ventas.
                </div>
            </div>
        </div>

        <!-- Mini footer -->
        <div class="text-center text-muted small mt-3">
            © {{ new Date().getFullYear() }} ProductSalesManager
        </div>
    </div>
</template>

<script setup>
    import { computed, onMounted, ref } from "vue";
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

    const filtered = computed(() => {
        const s = search.value.toLowerCase();
        if (!s) return items.value;

        return items.value.filter((x) => {
            const bySale = String(x.saleId ?? "").includes(s);
            const byProduct = String(x.productName ?? "").toLowerCase().includes(s);
            return bySale || byProduct;
        });
    });

    function clearMsgs() {
        error.value = "";
        success.value = "";
    }

    function normalizeApiError(e, fallback) {
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

    async function load() {
        clearMsgs();
        loading.value = true;
        try {
            const { data } = await api.get("/api/saleitems");
            items.value = data ?? [];
        } catch (e) {
            error.value = normalizeApiError(e, "Error cargando saleitems.");
        } finally {
            loading.value = false;
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
