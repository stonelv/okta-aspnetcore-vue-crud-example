jest.mock('@/TodosApiService', () => {
  const originalModule = jest.requireActual('@/TodosApiService')
  return {
    ...originalModule,
    default: {
      ...originalModule.default,
      execute: jest.fn()
    }
  }
})

describe('TodosApiService', () => {
  let api
  let mockExecute

  beforeEach(() => {
    jest.resetModules()
    const mock = jest.fn()
    mock.mockResolvedValue({})
    
    jest.doMock('axios', () => ({
      create: jest.fn(() => mock)
    }))
    
    jest.doMock('vue', () => {
      return {
        prototype: {
          $auth: {
            getAccessToken: jest.fn().mockResolvedValue('test-token')
          }
        }
      }
    })
    
    api = require('@/TodosApiService').default
    mockExecute = api.execute
  })

  describe('module structure', () => {
    it('should have all required methods', () => {
      expect(typeof api.execute).toBe('function')
      expect(typeof api.getList).toBe('function')
      expect(typeof api.getById).toBe('function')
      expect(typeof api.create).toBe('function')
      expect(typeof api.update).toBe('function')
      expect(typeof api.delete).toBe('function')
    })
  })

  describe('getList method', () => {
    it('should call execute with GET method and root path when no params', async () => {
      const executeSpy = jest.spyOn(api, 'execute').mockResolvedValue({})
      
      await api.getList()

      expect(executeSpy).toHaveBeenCalledWith('get', '/')
    })

    it('should build query string when params are provided', async () => {
      const executeSpy = jest.spyOn(api, 'execute').mockResolvedValue({})
      const params = {
        page: 2,
        pageSize: 20,
        isDone: true
      }

      await api.getList(params)

      expect(executeSpy).toHaveBeenCalled()
      const [method, url] = executeSpy.mock.calls[0]
      expect(method).toBe('get')
      expect(url).toContain('?')
      expect(url).toContain('page=2')
      expect(url).toContain('pageSize=20')
      expect(url).toContain('isDone=true')
    })

    it('should not include null or undefined params in query string', async () => {
      const executeSpy = jest.spyOn(api, 'execute').mockResolvedValue({})
      const params = {
        page: 1,
        isDone: null,
        undefinedParam: undefined
      }

      await api.getList(params)

      expect(executeSpy).toHaveBeenCalled()
      const [method, url] = executeSpy.mock.calls[0]
      expect(url).toContain('page=1')
      expect(url).not.toContain('isDone=null')
      expect(url).not.toContain('undefinedParam')
    })
  })

  describe('getById method', () => {
    it('should call execute with GET method and id in URL', async () => {
      const executeSpy = jest.spyOn(api, 'execute').mockResolvedValue({})
      const testId = 'test-todo-123'

      await api.getById(testId)

      expect(executeSpy).toHaveBeenCalledWith('get', `/${testId}`)
    })
  })

  describe('create method', () => {
    it('should call execute with POST method and data', async () => {
      const executeSpy = jest.spyOn(api, 'execute').mockResolvedValue({})
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
      const executeSpy = jest.spyOn(api, 'execute').mockResolvedValue({})
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
      const executeSpy = jest.spyOn(api, 'execute').mockResolvedValue({})
      const testId = 'test-todo-123'

      await api.delete(testId)

      expect(executeSpy).toHaveBeenCalledWith('delete', `/${testId}`)
    })
  })
})
