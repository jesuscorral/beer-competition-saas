# VSCode Debug Configuration

This directory contains VSCode configurations for debugging the Beer Competition Platform.

## 🐛 Debug Configurations

### 1. **Launch Chrome (Frontend)** ⭐ Recommended
- **Shortcut**: F5 (default)
- **What it does**: 
  - Automatically starts Vite dev server (`npm run dev`)
  - Opens Chrome with debugger attached
  - Sets breakpoints in your React components
- **Use when**: Starting fresh debugging session

### 2. **Attach to Chrome**
- **What it does**: Connects to an already running Chrome instance
- **Use when**: 
  - Vite server is already running
  - Want to reconnect debugger after restart
- **Requirements**: 
  - Start Chrome manually with: `chrome.exe --remote-debugging-port=9222`
  - Or just use "Launch Chrome" instead

### 3. **Launch Edge (Frontend)**
- Same as "Launch Chrome" but uses Microsoft Edge
- **Use when**: Prefer Edge over Chrome

### 4. **Debug Vite Server**
- **What it does**: Debugs the Vite build configuration and server
- **Use when**: 
  - Troubleshooting Vite config issues
  - Debugging build process
  - Not for debugging React components

### 5. **Full Stack Debug (Frontend + Server)** 🚀
- **Compound configuration**
- Starts both Vite server AND Chrome debugger together
- **Best for**: Complex debugging scenarios

## 📝 How to Use

### Quick Start
1. Open any `.tsx` file (e.g., `CompetitionForm.tsx`)
2. Click on the line number to set a breakpoint (red dot appears)
3. Press **F5** or go to **Run and Debug** panel
4. Select **"Launch Chrome (Frontend)"**
5. Chrome will open and hit your breakpoint when that code runs!

### Setting Breakpoints
- **Line breakpoints**: Click left of line number
- **Conditional breakpoints**: Right-click → Add Conditional Breakpoint
- **Logpoints**: Right-click → Add Logpoint (logs without stopping)

### Debug Panel Features
- **Variables**: See all variables in current scope
- **Watch**: Add expressions to monitor
- **Call Stack**: See the function call hierarchy
- **Breakpoints**: Manage all breakpoints
- **Console**: Execute code in current context

### Keyboard Shortcuts
- **F5**: Start debugging
- **F9**: Toggle breakpoint
- **F10**: Step over
- **F11**: Step into
- **Shift+F11**: Step out
- **Shift+F5**: Stop debugging
- **Ctrl+Shift+F5**: Restart debugging

## 🛠️ Tasks

### Available Tasks
Run via **Terminal → Run Task** or **Ctrl+Shift+B**:

1. **npm: dev** - Start Vite dev server
2. **npm: build** - Build for production
3. **Install Frontend Dependencies** - Run `npm install`
4. **Start Backend Services** - Start Docker containers

## 🔧 Troubleshooting

### Breakpoint not being hit?
- ✅ Check source maps are enabled in `vite.config.ts`
- ✅ Verify file path in breakpoint matches `webRoot` setting
- ✅ Refresh browser after setting new breakpoint
- ✅ Check Chrome DevTools → Sources to see if file is loaded

### "Cannot connect to runtime process"
- ✅ Wait a few seconds for Vite server to fully start
- ✅ Check port 5173 is not already in use: `netstat -ano | findstr :5173`
- ✅ Kill existing processes: `taskkill /F /PID <pid>`

### Source maps not working?
- ✅ Verify `sourceMaps: true` in `vite.config.ts`
- ✅ Clear Vite cache: Delete `node_modules/.vite`
- ✅ Restart VSCode

### Chrome won't launch?
- ✅ Check Chrome is installed and in PATH
- ✅ Try Edge configuration instead
- ✅ Start server manually: `npm run dev`, then use "Attach to Chrome"

## 📚 Tips & Best Practices

### Debugging React Components
```typescript
// Set breakpoints in lifecycle or hooks
useEffect(() => {
  debugger; // This also works! Pauses execution
  console.log('Component mounted');
}, []);

// Debug event handlers
const handleSubmit = (data: FormData) => {
  debugger; // Pause here when form submits
  createCompetition(data);
};
```

### Debugging API Calls
```typescript
// In useCompetitions.ts
mutationFn: async (data: CompetitionFormData) => {
  debugger; // Pause before API call
  const response = await apiClient.post('/api/competitions', data);
  debugger; // Pause after response
  return response.data;
},
```

### Watch Expressions
Add these to Watch panel:
- `data` - See form data
- `errors` - See validation errors  
- `response.status` - Check HTTP status
- `localStorage` - Inspect stored tokens

### Console Tricks
In Debug Console, you can:
```javascript
// Modify state on-the-fly
data.name = "Test Competition"

// Call functions
console.table(data)

// Check environment
import.meta.env.VITE_API_BASE_URL
```

## 🎯 Common Debugging Scenarios

### 1. Form Validation Not Working
```typescript
// In CompetitionForm.tsx
const onSubmit = (data: CompetitionFormData) => {
  debugger; // Check if validation passed
  // Step through to see where it fails
};
```

### 2. API Call Failing
```typescript
// In api/client.ts interceptor
apiClient.interceptors.response.use(
  (response) => {
    debugger; // Success path
    return response;
  },
  (error) => {
    debugger; // Error path - inspect error object
    return Promise.reject(error);
  }
);
```

### 3. Component Not Re-rendering
```typescript
// In CompetitionForm.tsx
const createCompetitionMutation = useCreateCompetition();

// Set breakpoint in onSuccess
onSuccess: () => {
  debugger; // Check if this runs after successful mutation
  queryClient.invalidateQueries({ queryKey: ['competitions'] });
}
```

## 📖 Resources

- [VSCode Debugging Docs](https://code.visualstudio.com/docs/editor/debugging)
- [Chrome DevTools](https://developer.chrome.com/docs/devtools/)
- [Vite Debug Guide](https://vitejs.dev/guide/debugging.html)
- [React DevTools](https://react.dev/learn/react-developer-tools)

---

**Pro Tip**: Install the "React Developer Tools" Chrome extension for additional React-specific debugging capabilities!
