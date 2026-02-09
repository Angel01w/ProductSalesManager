import api from "./api";

export const ProductsService = {
	getAll: () => api.get("/api/products"),
	create: (payload) => api.post("/api/products", payload),
	update: (id, payload) => api.put(`/api/products/${id}`, payload),
	remove: (id) => api.delete(`/api/products/${id}`),
};
