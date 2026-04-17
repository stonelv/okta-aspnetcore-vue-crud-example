import { http } from './http'

const API_BASE = '/Meals'

export const mealsApi = {
  getList (params = {}) {
    const { keyword, startDate, endDate, page, pageSize } = params
    return http.get(API_BASE, {
      keyword,
      startDate,
      endDate,
      page,
      pageSize
    })
  },
  
  getById (id) {
    return http.get(`${API_BASE}/${id}`)
  },
  
  create (data) {
    return http.post(API_BASE, data)
  },
  
  update (id, data) {
    return http.put(`${API_BASE}/${id}`, data)
  },
  
  delete (id) {
    return http.delete(`${API_BASE}/${id}`)
  }
}

export default mealsApi
