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
  getList (params = {}) {
    const queryString = Object.keys(params)
      .filter(key => params[key] !== null && params[key] !== undefined)
      .map(key => `${encodeURIComponent(key)}=${encodeURIComponent(params[key])}`)
      .join('&')
    const url = queryString ? `/?${queryString}` : '/'
    return this.execute('get', url)
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
