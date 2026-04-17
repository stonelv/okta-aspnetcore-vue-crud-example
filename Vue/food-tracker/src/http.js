import Vue from 'vue'
import axios from 'axios'

const http = axios.create({
  baseURL: 'http://localhost:5000/api',
  json: true,
  headers: {
    'Content-Type': 'application/json'
  }
})

http.interceptors.request.use(
  async (config) => {
    try {
      const accessToken = await Vue.prototype.$auth.getAccessToken()
      if (accessToken) {
        config.headers.Authorization = `Bearer ${accessToken}`
      }
    } catch (error) {
      console.log('No access token available:', error)
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

http.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    let message = 'An error occurred'
    
    if (error.response) {
      const { status, data } = error.response
      
      if (data && data.errors && data.errors.length > 0) {
        message = data.errors.join(', ')
      } else if (data && data.message) {
        message = data.message
      } else {
        switch (status) {
          case 400:
            message = 'Bad request. Please check your input.'
            break
          case 401:
            message = 'Unauthorized. Please login again.'
            break
          case 403:
            message = 'Forbidden. You do not have permission.'
            break
          case 404:
            message = 'Resource not found.'
            break
          case 500:
            message = 'Server error. Please try again later.'
            break
          default:
            message = `Error: ${status}`
        }
      }
    } else if (error.request) {
      message = 'Network error. Please check your connection.'
    }
    
    error.userMessage = message
    return Promise.reject(error)
  }
)

export default http
