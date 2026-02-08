import api from "./api";

export const CustomersService = {
	getAll: () => api.get("/api/customer"),
	create: (payload) => api.post("/api/customer", payload),
	update: (id, payload) => api.put(`/api/customer/${id}`, payload),
	remove: (id) => api.delete(`/api/customer/${id}`),
};
