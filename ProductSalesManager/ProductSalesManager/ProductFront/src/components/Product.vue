<!-- src/components/Product.vue -->
<template>
    <div class="container py-4">
        <!-- Top bar -->
        <div class="d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-2 mb-3">
            <div>
                <h4 class="mb-1">Productos</h4>
                <div class="text-muted small">Crear • Editar • Eliminar (API: /api/products)</div>
            </div>

            <div class="d-flex gap-2">
                <div class="input-group">
                    <span class="input-group-text">🔎</span>
                    <input v-model.trim="search" class="form-control" placeholder="Buscar por nombre..." />
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
                                <h6 class="mb-1">{{ isEdit ? "Editar producto" : "Nuevo producto" }}</h6>
                                <div class="text-muted small">
                                    {{ isEdit ? `ID #${form.id}` : "Completa el formulario y guarda" }}
                                </div>
                            </div>
                            <span class="badge" :class="isEdit ? 'text-bg-warning' : 'text-bg-dark'">
                                {{ isEdit ? "EDIT" : "CREATE" }}
                            </span>
                        </div>

                        <form @submit.prevent="onSubmit" novalidate>
                            <div class="mb-3">
                                <label class="form-label">Nombre</label>
                                <input v-model.trim="form.name"
                                       class="form-control"
                                       :class="{ 'is-invalid': !!errs.name }"
                                       placeholder="Ej: Laptop Dell" />
                                <div class="invalid-feedback">{{ errs.name }}</div>
                            </div>

                            <div class="mb-3">
                                <label class="form-label">Descripción</label>
                                <textarea v-model.trim="form.description"
                                          class="form-control"
                                          rows="3"
                                          placeholder="Opcional"></textarea>
                            </div>

                            <div class="row g-2">
                                <div class="col-6">
                                    <label class="form-label">Precio</label>
                                    <input v-model.number="form.price"
                                           type="number"
                                           step="0.01"
                                           class="form-control"
                                           :class="{ 'is-invalid': !!errs.price }"
                                           placeholder="0.00" />
                                    <div class="invalid-feedback">{{ errs.price }}</div>
                                </div>

                                <div class="col-6">
                                    <label class="form-label">Stock</label>
                                    <input v-model.number="form.stock"
                                           type="number"
                                           class="form-control"
                                           :class="{ 'is-invalid': !!errs.stock }"
                                           placeholder="0" />
                                    <div class="invalid-feedback">{{ errs.stock }}</div>
                                </div>
                            </div>

                            <div class="d-flex gap-2 mt-3">
                                <button class="btn btn-dark" type="submit" :disabled="saving">
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
                                <span>Endpoint</span>
                                <span class="fw-semibold">/api/products</span>
                            </div>
                            <div class="d-flex justify-content-between">
                                <span>Registros</span>
                                <span class="fw-semibold">{{ products.length }}</span>
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
                                        <th>Producto</th>
                                        <th class="text-end" style="width: 140px">Precio</th>
                                        <th class="text-end" style="width: 110px">Stock</th>
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
                                            No hay productos. Crea uno usando el formulario.
                                        </td>
                                    </tr>

                                    <tr v-else v-for="p in filtered" :key="p.id">
                                        <td class="fw-semibold">{{ p.id }}</td>

                                        <td>
                                            <div class="d-flex align-items-center gap-2">
                                                <span class="avatar">{{ initials(p.name) }}</span>
                                                <div class="min-w-0">
                                                    <div class="fw-semibold text-truncate">{{ p.name }}</div>
                                                    <div class="text-muted small text-truncate" style="max-width: 360px;">
                                                        {{ p.description || "Sin descripción" }}
                                                    </div>
                                                </div>
                                            </div>
                                        </td>

                                        <td class="text-end fw-semibold">{{ money(p.price) }}</td>
                                        <td class="text-end">{{ p.stock }}</td>

                                        <td class="text-end">
                                            <button class="btn btn-sm btn-outline-primary me-2" @click="edit(p)" :disabled="saving">
                                                Editar
                                            </button>
                                            <button class="btn btn-sm btn-outline-danger" @click="remove(p)" :disabled="saving">
                                                Eliminar
                                            </button>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>

                        <div class="text-muted small mt-2">
                            * Validaciones backend: Name requerido, Price/Stock no negativos.
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

const products = ref([]);

const form = reactive({
  id: null,
  name: "",
  description: "",
  price: 0,
  stock: 0,
});

const errs = reactive({
  name: "",
  price: "",
  stock: "",
});

const isEdit = computed(() => !!form.id);

const filtered = computed(() => {
  const s = search.value.toLowerCase();
  if (!s) return products.value;
  return products.value.filter((x) => (x.name || "").toLowerCase().includes(s));
});

function clearMsgs() {
  error.value = "";
  success.value = "";
}

function initials(name) {
  const parts = (name || "").trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "P";
  const first = parts[0]?.[0] ?? "";
  const last = parts.length > 1 ? parts[parts.length - 1]?.[0] ?? "" : "";
  return (first + last).toUpperCase();
}

function money(value) {
  const n = Number(value ?? 0);
  // Si quieres RD$ cambia a "DOP"
  return n.toLocaleString(undefined, { style: "currency", currency: "USD" });
}

function validate() {
  errs.name = "";
  errs.price = "";
  errs.stock = "";

  if (!form.name) errs.name = "El nombre es requerido.";
  if (form.price < 0) errs.price = "Price no puede ser negativo.";
  if (form.stock < 0) errs.stock = "Stock no puede ser negativo.";

  return !errs.name && !errs.price && !errs.stock;
}

function resetForm() {
  form.id = null;
  form.name = "";
  form.description = "";
  form.price = 0;
  form.stock = 0;

  errs.name = "";
  errs.price = "";
  errs.stock = "";
}

function normalizeApiError(e, fallback) {
  return e?.response?.data?.message || fallback;
}

async function load() {
  clearMsgs();
  loading.value = true;
  try {
    const { data } = await api.get("/api/products");
    products.value = data ?? [];
  } catch (e) {
    error.value = normalizeApiError(e, "Error cargando productos.");
  } finally {
    loading.value = false;
  }
}

function edit(p) {
  clearMsgs();
  form.id = p.id;
  form.name = p.name ?? "";
  form.description = p.description ?? "";
  form.price = Number(p.price ?? 0);
  form.stock = Number(p.stock ?? 0);
}

async function onSubmit() {
  clearMsgs();
  if (!validate()) return;

  saving.value = true;
  try {
    const payload = {
      name: form.name,
      description: form.description || null,
      price: form.price,
      stock: form.stock,
    };

    if (!isEdit.value) {
      await api.post("/api/products", payload);
      success.value = "Producto creado.";
    } else {
      await api.put(`/api/products/${form.id}`, payload);
      success.value = "Producto actualizado.";
    }

    resetForm();
    await load();
  } catch (e) {
    error.value = normalizeApiError(e, "Error guardando producto.");
  } finally {
    saving.value = false;
  }
}

async function remove(p) {
  clearMsgs();
  const ok = confirm(`¿Eliminar el producto "${p.name}"?`);
  if (!ok) return;

  saving.value = true;
  try {
    await api.delete(`/api/products/${p.id}`);
    success.value = "Producto eliminado.";
    await load();
  } catch (e) {
    error.value = normalizeApiError(e, "Error eliminando producto.");
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
        background: rgba(33, 37, 41, 0.12);
        color: rgb(33, 37, 41);
        flex: 0 0 auto;
    }

    .min-w-0 {
        min-width: 0;
    }
</style>
