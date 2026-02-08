import api from "./api";

export const SalesService = {
	getAll: () => api.get("/api/sales"),
	create: (payload) => api.post("/api/sales", payload),
	update: (id, payload) => api.put(`/api/sales/${id}`, payload),
	remove: (id) => api.delete(`/api/sales/${id}`),
};
