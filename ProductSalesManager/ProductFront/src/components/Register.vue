<template>
    <div class="auth-wrap min-vh-100 d-flex align-items-center py-5">
        <!-- fondo decorativo -->
        <div class="auth-bg">
            <div class="blob blob-1"></div>
            <div class="blob blob-2"></div>
            <div class="grid"></div>
        </div>

        <div class="container" style="max-width: 980px;">
            <div class="row align-items-stretch g-4">
                <!-- Panel izquierdo (branding) -->
                <div class="col-12 col-lg-6">
                    <div class="brand-card h-100 p-4 p-md-5">
                        <div class="d-flex align-items-center gap-2 mb-4">
                            <span class="brand-dot"></span>
                            <div class="fw-bold fs-5">ProductSalesManager</div>
                            <span class="badge rounded-pill text-bg-success ms-auto">Registro</span>
                        </div>

                        <h2 class="mb-2">Crea tu cuenta</h2>
                        <p class="text-muted mb-4">
                            Accede al panel para gestionar <span class="fw-semibold">clientes</span>,
                            <span class="fw-semibold">productos</span> y <span class="fw-semibold">ventas</span>.
                        </p>

                        <div class="features">
                            <div class="feat">
                                <div class="feat-ico">📦</div>
                                <div>
                                    <div class="fw-semibold">Inventario</div>
                                    <div class="text-muted small">Productos, precios y stock.</div>
                                </div>
                            </div>

                            <div class="feat">
                                <div class="feat-ico">🧾</div>
                                <div>
                                    <div class="fw-semibold">Ventas</div>
                                    <div class="text-muted small">Registra ventas y revisa historial.</div>
                                </div>
                            </div>

                            <div class="feat">
                                <div class="feat-ico">🔒</div>
                                <div>
                                    <div class="fw-semibold">Seguro</div>
                                    <div class="text-muted small">Autenticación con JWT.</div>
                                </div>
                            </div>
                        </div>

                        <div class="mt-auto pt-4 text-muted small">
                            © {{ new Date().getFullYear() }} ProductSalesManager
                        </div>
                    </div>
                </div>

                <!-- Panel derecho (form) -->
                <div class="col-12 col-lg-6">
                    <div class="card auth-card border-0 shadow-sm h-100">
                        <div class="card-body p-4 p-md-5">
                            <div class="d-flex align-items-start justify-content-between mb-3">
                                <div>
                                    <h4 class="mb-1">Registrarse</h4>
                                    <div class="text-muted">Crea tu usuario</div>
                                </div>
                                <span class="chip">CREATE</span>
                            </div>

                            <div v-if="error"
                                 class="alert alert-danger d-flex align-items-center justify-content-between gap-2"
                                 role="alert">
                                <div class="d-flex align-items-center gap-2">
                                    <span>⚠️</span>
                                    <span>{{ error }}</span>
                                </div>
                                <button class="btn-close" @click="error = ''"></button>
                            </div>

                            <div v-if="success"
                                 class="alert alert-success d-flex align-items-center justify-content-between gap-2"
                                 role="alert">
                                <div class="d-flex align-items-center gap-2">
                                    <span>✅</span>
                                    <span>{{ success }}</span>
                                </div>
                                <button class="btn-close" @click="success = ''"></button>
                            </div>

                            <form @submit.prevent="register" class="mt-3">
                                <div class="mb-3">
                                    <label class="form-label">Nombre</label>
                                    <div class="input-group input-soft">
                                        <span class="input-group-text">👤</span>
                                        <input v-model.trim="name" class="form-control" placeholder="Ej: Jose Perez" required />
                                    </div>
                                </div>

                                <div class="mb-3">
                                    <label class="form-label">Email</label>
                                    <div class="input-group input-soft">
                                        <span class="input-group-text">✉️</span>
                                        <input v-model.trim="email" type="email" class="form-control" placeholder="ejemplo@mail.com" required />
                                    </div>
                                </div>

                                <div class="mb-2">
                                    <label class="form-label">Contraseña</label>
                                    <div class="input-group input-soft">
                                        <span class="input-group-text">🔑</span>
                                        <input v-model="password" type="password" class="form-control" placeholder="••••••••" required />
                                    </div>
                                </div>

                                <div class="text-muted small mb-3">
                                    * Usa una contraseña segura. (Luego puedes mejorar reglas si quieres)
                                </div>

                                <button class="btn btn-success w-100 btn-cta" :disabled="loading">
                                    <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
                                    Crear cuenta
                                </button>
                            </form>

                            <div class="text-center mt-3">
                                <small class="text-muted">
                                    ¿Ya tienes cuenta? <router-link class="link-primary fw-semibold" to="/login">Inicia sesión</router-link>
                                </small>
                            </div>

                            <div class="divider my-4"></div>

                            <div class="text-muted small">
                                Al registrarte, se creará tu usuario y luego te llevará al login.
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- mini foot en móvil -->
            <div class="text-center text-muted small mt-3 d-lg-none">
                © {{ new Date().getFullYear() }} ProductSalesManager
            </div>
        </div>
    </div>
</template>

<script setup>
    import { ref } from "vue";
    import { useRouter } from "vue-router";
    import api from "@/services/api";

    const router = useRouter();

    const name = ref("");
    const email = ref("");
    const password = ref("");
    const loading = ref(false);
    const error = ref("");
    const success = ref("");

    async function register() {
        error.value = "";
        success.value = "";
        loading.value = true;

        try {
            const { data } = await api.post("/api/Auth/register", {
                name: name.value,
                email: email.value,
                password: password.value,
            });

            // si tu register devuelve token, guárdalo y entra
            if (data?.token) localStorage.setItem("token", data.token);

            success.value = "Usuario creado. Te llevaré al login.";
            setTimeout(() => router.push("/login"), 700);
        } catch (e) {
            error.value = e?.response?.data?.message || e?.message || "Error registrando.";
        } finally {
            loading.value = false;
        }
    }
</script>

<style scoped>
    .auth-wrap {
        position: relative;
        background: #f6f8fb;
        overflow: hidden;
    }

    /* Fondo decorativo */
    .auth-bg {
        position: absolute;
        inset: 0;
        pointer-events: none;
    }

    .grid {
        position: absolute;
        inset: -40px;
        background-image: linear-gradient(to right, rgba(13,110,253,.06) 1px, transparent 1px), linear-gradient(to bottom, rgba(13,110,253,.06) 1px, transparent 1px);
        background-size: 48px 48px;
        opacity: .55;
    }

    .blob {
        position: absolute;
        filter: blur(40px);
        opacity: .35;
        border-radius: 999px;
    }

    .blob-1 {
        width: 420px;
        height: 420px;
        left: -120px;
        top: -140px;
        background: radial-gradient(circle at 30% 30%, #198754, transparent 60%);
    }

    .blob-2 {
        width: 520px;
        height: 520px;
        right: -180px;
        bottom: -220px;
        background: radial-gradient(circle at 30% 30%, #0d6efd, transparent 60%);
    }

    /* Tarjetas */
    .brand-card {
        background: rgba(255,255,255,.72);
        border: 1px solid rgba(15, 23, 42, 0.08);
        border-radius: 18px;
        backdrop-filter: blur(8px);
        display: flex;
        flex-direction: column;
        min-height: 420px;
    }

    .auth-card {
        border-radius: 18px;
        background: rgba(255,255,255,.92);
        border: 1px solid rgba(15, 23, 42, 0.08);
        backdrop-filter: blur(8px);
    }

    .brand-dot {
        width: 12px;
        height: 12px;
        border-radius: 999px;
        background: #198754;
        display: inline-block;
    }

    .chip {
        font-size: 12px;
        font-weight: 800;
        letter-spacing: .4px;
        padding: 6px 10px;
        border-radius: 999px;
        background: rgba(25, 135, 84, .12);
        color: #198754;
    }

    /* Inputs */
    .input-soft .input-group-text {
        background: #f3f6fb;
        border: 1px solid rgba(15, 23, 42, 0.10);
        border-right: 0;
    }

    .input-soft .form-control {
        background: #ffffff;
        border: 1px solid rgba(15, 23, 42, 0.10);
    }

        .input-soft .form-control:focus {
            box-shadow: 0 0 0 .25rem rgba(25, 135, 84, .12);
            border-color: rgba(25, 135, 84, .35);
        }

    /* Botón principal */
    .btn-cta {
        border-radius: 12px;
        padding: 12px 14px;
        font-weight: 700;
    }

    /* Features */
    .features {
        display: grid;
        gap: 12px;
    }

    .feat {
        display: flex;
        gap: 12px;
        align-items: center;
        padding: 12px;
        border-radius: 14px;
        background: rgba(25, 135, 84, 0.06);
        border: 1px solid rgba(25, 135, 84, 0.10);
    }

    .feat-ico {
        width: 42px;
        height: 42px;
        border-radius: 14px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        background: rgba(25, 135, 84, 0.14);
    }

    /* Divider */
    .divider {
        height: 1px;
        background: rgba(15, 23, 42, 0.10);
    }
</style>
