<!-- src/components/TheWelcome.vue -->
<template>
    <div class="bg-light min-vh-100">
        <!-- Top Navbar -->
        <nav class="navbar navbar-expand-lg bg-white border-bottom">
            <div class="container py-2">
                <div class="d-flex align-items-center gap-2">
                    <span class="brand-dot"></span>
                    <div>
                        <div class="fw-bold lh-1">ProductSalesManager</div>
                        <div class="text-muted small lh-1">Panel administrativo</div>
                    </div>
                </div>

                <!-- ✅ DERECHA: badges + botones -->
                <div class="d-flex align-items-center gap-2">
                    <span class="badge text-bg-primary">Vue 3</span>
                    <span class="badge text-bg-dark">.NET API</span>

                    <div class="vr mx-2"></div>

                    <router-link class="btn btn-sm btn-primary" to="/login">
                        Iniciar sesión
                    </router-link>
                    <router-link class="btn btn-sm btn-outline-primary" to="/register">
                        Registrarse
                    </router-link>
                </div>
            </div>
        </nav>

        <div class="container-fluid py-4">
            <!-- Cards -->
            <div class="row g-3 mb-3">
                <div class="col-12 col-md-3">
                    <div class="card shadow-sm border-0 h-100">
                        <div class="card-body">
                            <div class="d-flex align-items-center justify-content-between">
                                <div>
                                    <div class="text-muted small">Módulo</div>
                                    <div class="fw-bold">Clientes</div>
                                </div>
                                <div class="pill">CRUD</div>
                            </div>
                            <div class="mt-3 text-muted small">Crear, editar y eliminar clientes.</div>
                            <button class="btn btn-primary w-100 mt-3" @click="tab = 'clientes'">
                                Ir a Clientes
                            </button>
                        </div>
                    </div>
                </div>

                <div class="col-12 col-md-3">
                    <div class="card shadow-sm border-0 h-100">
                        <div class="card-body">
                            <div class="d-flex align-items-center justify-content-between">
                                <div>
                                    <div class="text-muted small">Módulo</div>
                                    <div class="fw-bold">Productos</div>
                                </div>
                                <div class="pill">CRUD</div>
                            </div>
                            <div class="mt-3 text-muted small">Administrar productos (precio y stock).</div>
                            <button class="btn btn-dark w-100 mt-3" @click="tab = 'productos'">
                                Ir a Productos
                            </button>
                        </div>
                    </div>
                </div>

                <div class="col-12 col-md-3">
                    <div class="card shadow-sm border-0 h-100">
                        <div class="card-body">
                            <div class="d-flex align-items-center justify-content-between">
                                <div>
                                    <div class="text-muted small">Módulo</div>
                                    <div class="fw-bold">Ventas</div>
                                </div>
                                <div class="pill">CRUD</div>
                            </div>
                            <div class="mt-3 text-muted small">Registrar ventas y ver historial.</div>
                            <button class="btn btn-success w-100 mt-3" @click="tab = 'ventas'">
                                Ir a Ventas
                            </button>
                        </div>
                    </div>
                </div>

                <div class="col-12 col-md-3">
                    <div class="card shadow-sm border-0 h-100">
                        <div class="card-body">
                            <div class="d-flex align-items-center justify-content-between">
                                <div>
                                    <div class="text-muted small">Módulo</div>
                                    <div class="fw-bold">SaleItems</div>
                                </div>
                                <div class="pill">CRUD</div>
                            </div>
                            <div class="mt-3 text-muted small">Detalle de venta (ítems por venta).</div>
                            <button class="btn btn-info text-white w-100 mt-3" @click="tab = 'saleitems'">
                                Ir a SaleItems
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Tabs -->
            <div class="card shadow-sm border-0">
                <div class="card-body">
                    <div class="d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-2 mb-3">
                        <div>
                            <h5 class="mb-1">Subtareas</h5>
                            <div class="text-muted small">Selecciona el módulo a gestionar</div>
                        </div>

                        <div class="btn-group" role="group" aria-label="Tabs">
                            <button type="button"
                                    class="btn"
                                    :class="tab === 'clientes' ? 'btn-primary' : 'btn-outline-primary'"
                                    @click="tab = 'clientes'">
                                Clientes
                            </button>

                            <button type="button"
                                    class="btn"
                                    :class="tab === 'productos' ? 'btn-dark' : 'btn-outline-dark'"
                                    @click="tab = 'productos'">
                                Productos
                            </button>

                            <button type="button"
                                    class="btn"
                                    :class="tab === 'ventas' ? 'btn-success' : 'btn-outline-success'"
                                    @click="tab = 'ventas'">
                                Ventas
                            </button>

                            <button type="button"
                                    class="btn"
                                    :class="tab === 'saleitems' ? 'btn-info text-white' : 'btn-outline-info'"
                                    @click="tab = 'saleitems'">
                                SaleItems
                            </button>
                        </div>
                    </div>

                    <CustomersCrud v-if="tab === 'clientes'" :key="tab" />
                    <ProductsCrud v-else-if="tab === 'productos'" :key="tab" />
                    <SaleCrud v-else-if="tab === 'ventas'" :key="tab" />
                    <SaleItemCrud v-else :key="tab" />
                </div>
            </div>

            <div class="text-center text-muted small mt-3">
                © {{ new Date().getFullYear() }} ProductSalesManager
            </div>
        </div>
    </div>
</template>

<script setup>
    import { ref } from "vue";
    import CustomersCrud from "../components/Customer.vue";
    import ProductsCrud from "../components/Product.vue";
    import SaleCrud from "../components/Sale.vue";
    import SaleItemCrud from "../components/SaleItem.vue";

    const tab = ref("clientes");
</script>

<style scoped>
    .brand-dot {
        width: 12px;
        height: 12px;
        border-radius: 999px;
        background: #0d6efd;
        display: inline-block;
    }

    .pill {
        font-size: 12px;
        padding: 6px 10px;
        border-radius: 999px;
        background: rgba(13, 110, 253, 0.12);
        color: #0d6efd;
        font-weight: 700;
    }
</style>
