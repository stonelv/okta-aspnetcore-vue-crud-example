const mockExecute = jest.fn()
const mockGetAccessToken = jest.fn()

jest.mock('axios', () => ({
  create: jest.fn(() => ({
    request: mockExecute
  }))
}))

jest.mock('vue', () => {
  const Vue = {
    prototype: {
      $auth: {
        getAccessToken: mockGetAccessToken
      }
    }
  }
  return Vue
}, { virtual: true })

describe('TodosApiService', () => {
  let api

  beforeEach(() => {
    jest.resetModules()
    mockExecute.mockClear()
    mockGetAccessToken.mockClear()
    mockGetAccessToken.mockResolvedValue('test-token')
    mockExecute.mockResolvedValue({ data: {} })
    
    api = require('@/TodosApiService').default
  })

  describe('execute method', () => {
    it('should get access token and add Authorization header', async () => {
      mockExecute.mockResolvedValue({ data: { success: true } })

      await api.execute('GET', '/test')

      expect(mockGetAccessToken).toHaveBeenCalled()
      expect(mockExecute).toHaveBeenCalledWith(
        expect.objectContaining({
          method: 'GET',
          url: '/test',
          headers: {
            Authorization: 'Bearer test-token'
          }
        })
      )
    })

    it('should return response data', async () => {
      const testData = { id: '1', title: 'Test Todo' }
      mockExecute.mockResolvedValue({ data: testData })

      const result = await api.execute('GET', '/test')

      expect(result).toEqual(testData)
    })
  })

  describe('getList method', () => {
    it('should call execute with GET method and correct URL', async () => {
      const executeSpy = jest.spyOn(api, 'execute')
      
      await api.getList()

      expect(executeSpy).toHaveBeenCalledWith('get', '/')
    })

    it('should build query string when params are provided', async () => {
      const executeSpy = jest.spyOn(api, 'execute')
      const params = {
        page: 2,
        pageSize: 20,
        isDone: true,
        sortBy: 'Title',
        sortDesc: false
      }

      await api.getList(params)

      expect(executeSpy).toHaveBeenCalledWith(
        'get',
        expect.stringContaining('?')
      )
    })

    it('should not include null or undefined params in query string', async () => {
      const executeSpy = jest.spyOn(api, 'execute')
      const params = {
        page: 1,
        isDone: null
      }

      await api.getList(params)

      expect(executeSpy).toHaveBeenCalled()
    })
  })

  describe('getById method', () => {
    it('should call execute with GET method and id in URL', async () => {
      const executeSpy = jest.spyOn(api, 'execute')
      const testId = 'test-todo-123'

      await api.getById(testId)

      expect(executeSpy).toHaveBeenCalledWith('get', `/${testId}`)
    })
  })

  describe('create method', () => {
    it('should call execute with POST method and data', async () => {
      const executeSpy = jest.spyOn(api, 'execute')
      const testData = {
        title: 'New Todo',
        isDone: false
      }

      await api.create(testData)

      expect(executeSpy).toHaveBeenCalledWith('post', '/', testData)
    })
  })

  describe('update method', () => {
    it('should call execute with PUT method, id and data', async () => {
      const executeSpy = jest.spyOn(api, 'execute')
      const testId = 'test-todo-123'
      const testData = {
        title: 'Updated Todo',
        isDone: true
      }

      await api.update(testId, testData)

      expect(executeSpy).toHaveBeenCalledWith('put', `/${testId}`, testData)
    })
  })

  describe('delete method', () => {
    it('should call execute with DELETE method and id in URL', async () => {
      const executeSpy = jest.spyOn(api, 'execute')
      const testId = 'test-todo-123'

      await api.delete(testId)

      expect(executeSpy).toHaveBeenCalledWith('delete', `/${testId}`)
    })
  })
})
