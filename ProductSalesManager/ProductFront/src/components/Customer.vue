<template>
    <div class="container py-4">
        <!-- Top bar -->
        <div class="d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-2 mb-3">
            <div>
                <h4 class="mb-1">Clientes</h4>
                <div class="text-muted small">Crear • Editar • Eliminar (API: /api/customer)</div>
            </div>

            <div class="d-flex gap-2">
                <div class="input-group">
                    <span class="input-group-text">🔎</span>
                    <input v-model.trim="search" class="form-control" placeholder="Buscar por nombre o email..." />
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
                                <h6 class="mb-1">{{ isEdit ? "Editar cliente" : "Nuevo cliente" }}</h6>
                                <div class="text-muted small">
                                    {{ isEdit ? `ID #${form.id}` : "Completa el formulario y guarda" }}
                                </div>
                            </div>
                            <span class="badge" :class="isEdit ? 'text-bg-warning' : 'text-bg-primary'">
                                {{ isEdit ? "EDIT" : "CREATE" }}
                            </span>
                        </div>

                        <form @submit.prevent="onSubmit" novalidate>
                            <div class="mb-3">
                                <label class="form-label">Nombre</label>
                                <input v-model.trim="form.name"
                                       class="form-control"
                                       :class="{ 'is-invalid': !!errs.name }"
                                       placeholder="Ej: Juan Pérez" />
                                <div class="invalid-feedback">{{ errs.name }}</div>
                            </div>

                            <div class="mb-3">
                                <label class="form-label">Email</label>
                                <input v-model.trim="form.email"
                                       type="email"
                                       class="form-control"
                                       :class="{ 'is-invalid': !!errs.email }"
                                       placeholder="ejemplo@mail.com" />
                                <div class="invalid-feedback">{{ errs.email }}</div>
                            </div>

                            <div class="mb-3">
                                <label class="form-label">Teléfono</label>
                                <input v-model.trim="form.phone" class="form-control" placeholder="Opcional" />
                            </div>

                            <div class="d-flex gap-2">
                                <button class="btn btn-primary" type="submit" :disabled="saving">
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
                                <span class="fw-semibold">{{ apiBase }}</span>
                            </div>
                            <div class="d-flex justify-content-between">
                                <span>Endpoint</span>
                                <span class="fw-semibold">/api/customer</span>
                            </div>
                            <div class="d-flex justify-content-between">
                                <span>Registros</span>
                                <span class="fw-semibold">{{ customers.length }}</span>
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
                                <button class="btn btn-outline-dark" @click="resetForm" :disabled="saving">
                                    Limpiar formulario
                                </button>
                            </div>
                        </div>

                        <div class="table-responsive">
                            <table class="table table-hover align-middle">
                                <thead class="table-light">
                                    <tr>
                                        <th style="width: 70px">ID</th>
                                        <th>Cliente</th>
                                        <th>Email</th>
                                        <th>Teléfono</th>
                                        <th class="text-end" style="width: 190px">Acciones</th>
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
                                            No hay clientes. Crea uno usando el formulario.
                                        </td>
                                    </tr>

                                    <tr v-else v-for="c in filtered" :key="c.id">
                                        <td class="fw-semibold">{{ c.id }}</td>

                                        <td>
                                            <div class="d-flex align-items-center gap-2">
                                                <span class="avatar">{{ initials(c.name) }}</span>
                                                <div class="min-w-0">
                                                    <div class="fw-semibold text-truncate">{{ c.name }}</div>
                                                    <div class="text-muted small">Cliente</div>
                                                </div>
                                            </div>
                                        </td>

                                        <td class="text-truncate" style="max-width: 220px">{{ c.email }}</td>
                                        <td>{{ c.phone || "-" }}</td>

                                        <td class="text-end">
                                            <button class="btn btn-sm btn-outline-primary me-2" @click="edit(c)" :disabled="saving">
                                                Editar
                                            </button>
                                            <button class="btn btn-sm btn-outline-danger" @click="remove(c)" :disabled="saving">
                                                Eliminar
                                            </button>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>

                        <div class="text-muted small mt-2">
                            * Si el backend devuelve <span class="fw-semibold">409</span>, es porque el email ya existe (Conflict).
                        </div>
                    </div>
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
    import { computed, onMounted, reactive, ref } from "vue";
    import axios from "axios";

    // ✅ lee del .env
	const apiBase = import.meta.env.VITE_API_BASE_URL || "https://localhost:7276";

    const api = axios.create({
        baseURL: apiBase,
        headers: { "Content-Type": "application/json" },
    });

    const loading = ref(false);
    const saving = ref(false);
    const error = ref("");
    const success = ref("");
    const search = ref("");

    const customers = ref([]);

    const form = reactive({
        id: null,
        name: "",
        email: "",
        phone: "",
    });

    const errs = reactive({
        name: "",
        email: "",
    });

    const isEdit = computed(() => !!form.id);

    const filtered = computed(() => {
        const s = search.value.toLowerCase();
        if (!s) return customers.value;

        return customers.value.filter(
            (x) =>
                (x.name || "").toLowerCase().includes(s) ||
                (x.email || "").toLowerCase().includes(s)
        );
    });

    function clearMsgs() {
        error.value = "";
        success.value = "";
    }

    function initials(name) {
        const parts = (name || "").trim().split(/\s+/).filter(Boolean);
        if (parts.length === 0) return "U";
        const first = parts[0]?.[0] ?? "";
        const last = parts.length > 1 ? parts[parts.length - 1]?.[0] ?? "" : "";
        return (first + last).toUpperCase();
    }

    function validate() {
        errs.name = "";
        errs.email = "";

        if (!form.name) errs.name = "El nombre es requerido.";
        if (!form.email) errs.email = "El email es requerido.";
        else if (!/^\S+@\S+\.\S+$/.test(form.email)) errs.email = "Email inválido.";

        return !errs.name && !errs.email;
    }

    function resetForm() {
        form.id = null;
        form.name = "";
        form.email = "";
        form.phone = "";
        errs.name = "";
        errs.email = "";
    }

    function normalizeApiError(e, fallback) {
        // Tu backend responde { message: "..." } en 400/404/409
        // Si es CORS/SSL a veces no viene response, por eso fallback.
        return e?.response?.data?.message || e?.message || fallback;
    }

    async function load() {
        clearMsgs();
        loading.value = true;
        try {
            const { data } = await api.get("/api/customer");
            customers.value = data ?? [];
        } catch (e) {
            error.value = normalizeApiError(e, "Error cargando clientes.");
        } finally {
            loading.value = false;
        }
    }

    function edit(c) {
        clearMsgs();
        form.id = c.id;
        form.name = c.name ?? "";
        form.email = c.email ?? "";
        form.phone = c.phone ?? "";
    }

    async function onSubmit() {
        clearMsgs();
        if (!validate()) return;

        saving.value = true;
        try {
            const payload = {
                name: form.name,
                email: form.email,
                phone: form.phone || null,
            };

            if (!isEdit.value) {
                await api.post("/api/customer", payload);
                success.value = "Cliente creado.";
            } else {
                await api.put(`/api/customer/${form.id}`, payload);
                success.value = "Cliente actualizado.";
            }

            resetForm();
            await load();
        } catch (e) {
            error.value = normalizeApiError(e, "Error guardando cliente.");
        } finally {
            saving.value = false;
        }
    }

    async function remove(c) {
        clearMsgs();
        const ok = confirm(`¿Eliminar el cliente "${c.name}"?`);
        if (!ok) return;

        saving.value = true;
        try {
            await api.delete(`/api/customer/${c.id}`);
            success.value = "Cliente eliminado.";
            await load();
        } catch (e) {
            error.value = normalizeApiError(e, "Error eliminando cliente.");
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
        background: rgba(13, 110, 253, 0.15);
        color: rgb(13, 110, 253);
        flex: 0 0 auto;
    }

    .min-w-0 {
        min-width: 0;
    }
</style>
