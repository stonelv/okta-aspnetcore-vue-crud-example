describe('Todos Component Logic', () => {
  describe('formatDate', () => {
    function formatDate(dateStr) {
      if (!dateStr) return '-'
      const date = new Date(dateStr)
      return date.toLocaleDateString()
    }

    it('returns "-" for null date', () => {
      expect(formatDate(null)).toBe('-')
    })

    it('returns "-" for undefined date', () => {
      expect(formatDate(undefined)).toBe('-')
    })

    it('formats valid date string', () => {
      const result = formatDate('2026-04-14T12:00:00Z')
      expect(result).not.toBe('-')
      expect(typeof result).toBe('string')
    })
  })

  describe('isOverdue', () => {
    function isOverdue(todo) {
      if (!todo.dueDate || todo.isDone) return false
      const dueDate = new Date(todo.dueDate)
      const today = new Date()
      today.setHours(0, 0, 0, 0)
      return dueDate < today
    }

    it('returns true for past due date and not done', () => {
      const todo = {
        id: '1',
        title: 'Overdue Todo',
        isDone: false,
        dueDate: '2020-01-01T00:00:00Z',
        createdAt: '2020-01-01T00:00:00Z'
      }
      expect(isOverdue(todo)).toBe(true)
    })

    it('returns false for future due date', () => {
      const todo = {
        id: '2',
        title: 'Future Todo',
        isDone: false,
        dueDate: '2030-01-01T00:00:00Z',
        createdAt: '2020-01-01T00:00:00Z'
      }
      expect(isOverdue(todo)).toBe(false)
    })

    it('returns false for completed todo even if past due', () => {
      const todo = {
        id: '3',
        title: 'Completed Todo',
        isDone: true,
        dueDate: '2020-01-01T00:00:00Z',
        createdAt: '2020-01-01T00:00:00Z'
      }
      expect(isOverdue(todo)).toBe(false)
    })

    it('returns false for todo without due date', () => {
      const todo = {
        id: '4',
        title: 'No Due Date Todo',
        isDone: false,
        dueDate: null,
        createdAt: '2020-01-01T00:00:00Z'
      }
      expect(isOverdue(todo)).toBe(false)
    })
  })

  describe('resetForm', () => {
    function createInitialState() {
      return {
        form: {
          title: '',
          dueDate: '',
          isDone: false
        },
        editingTodo: null,
        showModal: false
      }
    }

    function resetForm(state) {
      state.form = {
        title: '',
        dueDate: '',
        isDone: false
      }
      state.editingTodo = null
    }

    it('resets form to initial state', () => {
      const state = createInitialState()
      state.form.title = 'Test Todo'
      state.form.isDone = true
      state.form.dueDate = '2026-04-15'
      state.editingTodo = { id: '1', title: 'Existing' }

      resetForm(state)

      expect(state.form.title).toBe('')
      expect(state.form.isDone).toBe(false)
      expect(state.form.dueDate).toBe('')
      expect(state.editingTodo).toBeNull()
    })
  })

  describe('openModal', () => {
    function createInitialState() {
      return {
        form: {
          title: '',
          dueDate: '',
          isDone: false
        },
        editingTodo: null,
        showModal: false
      }
    }

    function formatDateForInput(dateStr) {
      if (!dateStr) return ''
      const date = new Date(dateStr)
      return date.toISOString().split('T')[0]
    }

    function openModal(state, todo = null) {
      state.editingTodo = todo
      if (todo) {
        state.form = {
          title: todo.title,
          dueDate: todo.dueDate ? formatDateForInput(todo.dueDate) : '',
          isDone: todo.isDone
        }
      } else {
        state.form = {
          title: '',
          dueDate: '',
          isDone: false
        }
      }
      state.showModal = true
    }

    it('opens modal for new todo with empty form', () => {
      const state = createInitialState()
      
      openModal(state)

      expect(state.showModal).toBe(true)
      expect(state.editingTodo).toBeNull()
      expect(state.form.title).toBe('')
      expect(state.form.isDone).toBe(false)
    })

    it('opens modal for editing todo with populated form', () => {
      const state = createInitialState()
      const existingTodo = {
        id: '1',
        title: 'Existing Todo',
        isDone: true,
        dueDate: '2026-04-15T00:00:00Z',
        createdAt: '2026-04-14T00:00:00Z'
      }

      openModal(state, existingTodo)

      expect(state.showModal).toBe(true)
      expect(state.editingTodo).toEqual(existingTodo)
      expect(state.form.title).toBe('Existing Todo')
      expect(state.form.isDone).toBe(true)
      expect(state.form.dueDate).toBe('2026-04-15')
    })
  })

  describe('formatDateForInput', () => {
    function formatDateForInput(dateStr) {
      if (!dateStr) return ''
      const date = new Date(dateStr)
      return date.toISOString().split('T')[0]
    }

    it('returns empty string for null', () => {
      expect(formatDateForInput(null)).toBe('')
    })

    it('returns empty string for undefined', () => {
      expect(formatDateForInput(undefined)).toBe('')
    })

    it('formats date to YYYY-MM-DD', () => {
      const result = formatDateForInput('2026-04-15T12:30:00Z')
      expect(result).toBe('2026-04-15')
    })
  })
})
