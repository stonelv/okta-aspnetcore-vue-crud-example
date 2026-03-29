import Vue from 'vue'
import axios from 'axios'

const client = axios.create({
  baseURL: 'http://localhost:5000/api/Todos',
  json: true
})

export default {
  async execute (method, resource, data) {
    const accessToken = await Vue.prototype.$auth.getAccessToken()
    return client({
      method,
      url: resource,
      data,
      headers: {
        Authorization: `Bearer ${accessToken}`
      }
    }).then(req => {
      return req.data
    })
  },
  getAll (params = {}) {
    return this.execute('get', '/', { params })
  },
  getById (id) {
    return this.execute('get', `/${id}`)
  },
  create (data) {
    return this.execute('post', '/', data)
  },
  update (id, data) {
    return this.execute('put', `/${id}`, data)
  },
  delete (id) {
    return this.execute('delete', `/${id}`)
  }
}
