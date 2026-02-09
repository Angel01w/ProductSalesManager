import api from "./api";

export const SaleItemsService = {
	getAll: () => api.get("/api/saleitems"),
	create: (payload) => api.post("/api/saleitems", payload),
	update: (id, payload) => api.put(`/api/saleitems/${id}`, payload),
	remove: (id) => api.delete(`/api/saleitems/${id}`),
};
