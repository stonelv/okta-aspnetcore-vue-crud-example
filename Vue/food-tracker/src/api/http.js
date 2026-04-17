import Vue from 'vue'
import axios from 'axios'

const BASE_URL = 'http://localhost:5000/api'

const httpClient = axios.create({
  baseURL: BASE_URL,
  json: true
})

httpClient.interceptors.request.use(
  async (config) => {
    const accessToken = await Vue.prototype.$auth.getAccessToken()
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

httpClient.interceptors.response.use(
  (response) => {
    return response.data
  },
  (error) => {
    let errorMessage = '网络请求失败'
    
    if (error.response) {
      var data = error.response.data || {}
      switch (error.response.status) {
        case 400:
          errorMessage = data.error || '请求参数错误'
          break
        case 401:
          errorMessage = '未授权，请重新登录'
          break
        case 404:
          errorMessage = data.message || '资源不存在'
          break
        case 500:
          errorMessage = data.error || '服务器内部错误'
          break
        default:
          errorMessage = '请求失败: ' + error.response.status
      }
    } else if (error.request) {
      errorMessage = '服务器无响应，请检查网络连接'
    }
    
    return Promise.reject(new Error(errorMessage))
  }
)

export const http = {
  get (url, params = {}) {
    const queryString = Object.keys(params)
      .filter(key => params[key] !== null && params[key] !== undefined && params[key] !== '')
      .map(key => `${encodeURIComponent(key)}=${encodeURIComponent(params[key])}`)
      .join('&')
    const requestUrl = queryString ? `${url}?${queryString}` : url
    return httpClient.get(requestUrl)
  },
  
  post (url, data = {}) {
    return httpClient.post(url, data)
  },
  
  put (url, data = {}) {
    return httpClient.put(url, data)
  },
  
  delete (url) {
    return httpClient.delete(url)
  }
}

export default httpClient
