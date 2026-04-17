import http from '@/http'

export default {
  async execute (method, resource, data, params) {
    const response = await http({
      method,
      url: resource,
      data,
      params
    })
    
    if (response.data && response.data.success) {
      return response.data.data
    }
    
    if (response.data && !response.data.success) {
      const error = new Error(response.data.message || 'Request failed')
      error.errors = response.data.errors
      throw error
    }
    
    return response.data
  },
  
  getList (params = {}) {
    return this.execute('get', '/Meals', null, params)
  },
  
  getById (id) {
    return this.execute('get', `/Meals/${id}`)
  },
  
  create (data) {
    return this.execute('post', '/Meals', data)
  },
  
  update (id, data) {
    return this.execute('put', `/Meals/${id}`, data)
  },
  
  delete (id) {
    return this.execute('delete', `/Meals/${id}`)
  }
}
