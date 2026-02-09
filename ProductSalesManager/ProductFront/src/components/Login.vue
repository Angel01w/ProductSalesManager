<template>
    <div class="auth-wrap">
        <!-- Fondo -->
        <div class="auth-bg"></div>

        <div class="container py-5">
            <div class="row justify-content-center">
                <div class="col-12 col-md-9 col-lg-6 col-xl-5">
                    <div class="card auth-card border-0 shadow-lg">
                        <div class="card-body p-4 p-md-5">
                            <!-- Header -->
                            <div class="d-flex align-items-center gap-3 mb-4">
                                <div class="brand-mark">
                                    <span class="dot"></span>
                                </div>
                                <div class="min-w-0">
                                    <h4 class="mb-0 fw-bold">ProductSalesManager</h4>
                                    <div class="text-muted small">Accede para administrar ventas y productos</div>
                                </div>
                            </div>

                            <!-- Title -->
                            <div class="mb-3">
                                <h5 class="mb-1 fw-bold">Iniciar sesión</h5>
                                <div class="text-muted small">Usa tu email y contraseña</div>
                            </div>

                            <!-- Alert -->
                            <div v-if="error"
                                 class="alert alert-danger d-flex align-items-center justify-content-between gap-2"
                                 role="alert">
                                <div class="d-flex align-items-center gap-2">
                                    <span class="icon-badge danger">!</span>
                                    <span class="small mb-0">{{ error }}</span>
                                </div>
                                <button type="button" class="btn-close" aria-label="Close" @click="error = ''"></button>
                            </div>

                            <form @submit.prevent="login" novalidate>
                                <!-- Email -->
                                <div class="mb-3">
                                    <label class="form-label">Email</label>
                                    <div class="input-group input-soft">
                                        <span class="input-group-text">📧</span>
                                        <input v-model.trim="email"
                                               type="email"
                                               class="form-control"
                                               placeholder="ejemplo@mail.com"
                                               autocomplete="email"
                                               required />
                                    </div>
                                </div>

                                <!-- Password -->
                                <div class="mb-2">
                                    <label class="form-label">Contraseña</label>
                                    <div class="input-group input-soft">
                                        <span class="input-group-text">🔒</span>
                                        <input v-model="password"
                                               :type="showPass ? 'text' : 'password'"
                                               class="form-control"
                                               placeholder="••••••••"
                                               autocomplete="current-password"
                                               required />
                                        <button class="btn btn-outline-secondary"
                                                type="button"
                                                @click="showPass = !showPass"
                                                :disabled="loading">
                                            {{ showPass ? "Ocultar" : "Ver" }}
                                        </button>
                                    </div>
                                </div>

                                <!-- Small row -->
                                <div class="d-flex justify-content-between align-items-center mt-2 mb-4">
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="remember" v-model="remember" />
                                        <label class="form-check-label small text-muted" for="remember">Recordarme</label>
                                    </div>
                                    <button class="btn btn-link p-0 small text-decoration-none" type="button" @click="forgot()">
                                        ¿Olvidaste tu contraseña?
                                    </button>
                                </div>

                                <!-- Button -->
                                <button class="btn btn-primary w-100 btn-hero" :disabled="loading">
                                    <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
                                    Entrar
                                </button>

                                <!-- Footer -->
                                <div class="text-center mt-3">
                                    <small class="text-muted">
                                        ¿No tienes cuenta?
                                        <router-link class="fw-semibold text-decoration-none" to="/register">Regístrate</router-link>
                                    </small>
                                </div>

                                <div class="divider my-4"></div>

                                <!-- Mini badges -->
                                <div class="d-flex justify-content-center gap-2 flex-wrap">
                                    <span class="badge rounded-pill text-bg-primary">Vue 3</span>
                                    <span class="badge rounded-pill text-bg-dark">.NET API</span>
                                    <span class="badge rounded-pill text-bg-success">JWT</span>
                                </div>
                            </form>
                        </div>

                        <div class="card-footer border-0 bg-transparent pb-4 pt-0">
                            <div class="text-center text-muted small">
                                © {{ new Date().getFullYear() }} ProductSalesManager
                            </div>
                        </div>
                    </div>

                    <!-- mini hint -->
                    <div class="text-center mt-3 small text-muted">
                        Tip: si ves <b>Network Error</b>, revisa CORS (origen: <code>http://localhost:5174</code>) y HTTPS.
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
    import { ref } from "vue";
    import { useRouter } from "vue-router";
    import api from "@/services/api";

    const router = useRouter();

    const email = ref("");
    const password = ref("");
    const loading = ref(false);
    const error = ref("");

    const showPass = ref(false);
    const remember = ref(true);

    async function login() {
        error.value = "";
        loading.value = true;
        try {
            const { data } = await api.post("/api/Auth/login", {
                email: email.value,
                password: password.value,
            });

            // guardar token
            localStorage.setItem("token", data.token);

            // opcional: guardar user
            localStorage.setItem("user", JSON.stringify(data.user));

           

            router.push("/"); 
        } catch (e) {
            error.value = e?.response?.data?.message || e?.message || "Error en login.";
        } finally {
            loading.value = false;
        }
    }

    function forgot() {
        alert("Pendiente: pantalla de recuperación.");
    }
</script>

<style scoped>
    .auth-wrap {
        position: relative;
        min-height: 100vh;
        overflow: hidden;
        background: #f6f7fb;
    }

    .auth-bg {
        position: absolute;
        inset: 0;
        background: radial-gradient(900px 500px at 20% 10%, rgba(13,110,253,.18), transparent 60%), radial-gradient(900px 500px at 80% 20%, rgba(25,135,84,.14), transparent 55%), radial-gradient(700px 420px at 50% 100%, rgba(32,201,151,.10), transparent 60%);
        pointer-events: none;
    }

    .auth-card {
        border-radius: 18px;
        backdrop-filter: blur(6px);
    }

    .brand-mark {
        width: 44px;
        height: 44px;
        border-radius: 14px;
        display: grid;
        place-items: center;
        background: rgba(13, 110, 253, 0.12);
        border: 1px solid rgba(13, 110, 253, 0.18);
    }

        .brand-mark .dot {
            width: 14px;
            height: 14px;
            border-radius: 999px;
            background: #0d6efd;
            box-shadow: 0 0 0 6px rgba(13, 110, 253, 0.14);
        }

    .input-soft .input-group-text {
        background: #f3f5fb;
        border-color: #e7eaf3;
    }

    .input-soft .form-control {
        border-color: #e7eaf3;
    }

        .input-soft .form-control:focus {
            box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.15);
            border-color: rgba(13, 110, 253, 0.35);
        }

    .btn-hero {
        padding: 0.8rem 1rem;
        font-weight: 700;
        border-radius: 12px;
    }

    .icon-badge {
        width: 22px;
        height: 22px;
        border-radius: 8px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        font-weight: 800;
        font-size: 12px;
    }

        .icon-badge.danger {
            background: rgba(220, 53, 69, 0.15);
            color: #dc3545;
        }

    .divider {
        height: 1px;
        background: linear-gradient(90deg, transparent, rgba(0,0,0,.10), transparent);
    }

    .min-w-0 {
        min-width: 0;
    }
</style>
