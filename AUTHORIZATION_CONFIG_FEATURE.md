# Authorization Configuration Feature

## ? What Was Added

I've added a **Configuration Dialog** with **Authorization Header** support to the Swagger UI. This allows you to set an `Authorization` header that will be automatically included in all API requests.

## ?? Features

### 1. **Configuration Button**
- Located in the header next to "View Raw JSON"
- Icon: ?? Configure
- Shows active state (green) when authorization is configured
- Tooltip shows current configuration status

### 2. **Configuration Modal**
```
????????????????????????????????????????????????
? ?? API Configuration                    [×] ?
????????????????????????????????????????????????
?                                              ?
? Authorization Header                         ?
? ?????????????????????????????????????????????
? ? Bearer your-token-here                  ??
? ?????????????????????????????????????????????
? This header will be added to all API        ?
? requests.                                    ?
? Example: Bearer eyJhbGciOiJIUzI1NiIs...      ?
?                                              ?
? ? Enable Authorization Header               ?
?                                              ?
????????????????????????????????????????????????
?              [Cancel]  [?? Save Configuration]?
????????????????????????????????????????????????
```

### 3. **Persistent Storage**
- Configuration saved to `localStorage`
- Persists across browser sessions
- Automatically loaded on page load

### 4. **Automatic Header Injection**
- When enabled, adds `Authorization` header to all API requests
- Applies to all endpoint testing
- Can be toggled on/off without losing the value

## ?? How to Use

### Step 1: Open Configuration
```
1. Click the "?? Configure" button in the header
2. Modal opens with configuration options
```

### Step 2: Set Authorization Header
```
1. Enter your authorization header in the text field
   Example: "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
   
2. Check "Enable Authorization Header" checkbox

3. Click "?? Save Configuration"
```

### Step 3: Test API
```
1. All subsequent API requests will include the Authorization header
2. Config button turns green to indicate it's active
3. Test any endpoint - authorization is automatically included
```

## ?? Technical Implementation

### Configuration State
```javascript
let apiConfig = {
    authorizationHeader: '',  // The actual header value
    authEnabled: false        // Whether to include it in requests
};
```

### Storage
```javascript
// Save to localStorage
localStorage.setItem('zeroreflection-api-config', JSON.stringify(apiConfig));

// Load from localStorage
const saved = localStorage.getItem('zeroreflection-api-config');
apiConfig = JSON.parse(saved);
```

### Header Injection
```javascript
function getRequestHeaders() {
    const headers = { 'Content-Type': 'application/json' };
    
    if (apiConfig.authEnabled && apiConfig.authorizationHeader) {
        headers['Authorization'] = apiConfig.authorizationHeader;
    }
    
    return headers;
}

// Used in fetch requests
const options = {
    method: endpoint.Method,
    headers: getRequestHeaders()  // ? Includes Authorization if configured
};
```

### Visual Feedback
```javascript
function updateConfigButton() {
    const btn = document.getElementById('config-btn');
    if (apiConfig.authEnabled && apiConfig.authorizationHeader) {
        btn.classList.add('active');  // Green background
        btn.title = 'Authorization configured';
    } else {
        btn.classList.remove('active');  // Default appearance
        btn.title = 'Configure API settings';
    }
}
```

## ?? UI Components

### Configuration Button
```css
.btn-config {
    background-color: rgba(255,255,255,0.2);
    border-color: rgba(255,255,255,0.4);
    color: white;
}

.btn-config.active {
    background-color: #48bb78;  /* Green when configured */
    border-color: #48bb78;
}
```

### Modal Styling
- Uses Bootstrap modal component
- Standard form controls
- Clear help text with examples
- Success feedback on save

## ?? Use Cases

### 1. **JWT Bearer Tokens**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

### 2. **API Keys**
```
Authorization: ApiKey abc123xyz789
```

### 3. **Basic Auth**
```
Authorization: Basic dXNlcm5hbWU6cGFzc3dvcmQ=
```

### 4. **Custom Schemes**
```
Authorization: Custom-Scheme your-credentials-here
```

## ?? Security Features

### 1. **LocalStorage Only**
- Stored locally in browser
- Not sent to server
- Cleared when clearing browser data

### 2. **Toggle Control**
- Can disable without losing the value
- Easy to turn on/off for testing

### 3. **No Auto-Complete**
- Text field doesn't save to browser autocomplete
- Sensitive data not exposed in form history

### 4. **Visual Indicators**
- Clear indication when auth is active
- Button color changes to green

## ?? Example Workflow

### Testing Authenticated API

**Step 1: Get Token**
```bash
# Login to get token
POST /api/auth/login
{
  "username": "admin",
  "password": "password"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

**Step 2: Configure Swagger UI**
```
1. Click "?? Configure"
2. Enter: "Bearer eyJhbGciOiJIUzI1NiIs..."
3. Check "Enable Authorization Header"
4. Click "?? Save Configuration"
```

**Step 3: Test Protected Endpoints**
```
1. Navigate to any protected endpoint
2. Enter parameters
3. Click "? Execute"
4. Request includes Authorization header automatically! ?
```

### Before vs After

**Before (Without Config):**
```
GET /api/users
Headers:
  Content-Type: application/json

Response: 401 Unauthorized
```

**After (With Config):**
```
GET /api/users
Headers:
  Content-Type: application/json
  Authorization: Bearer eyJhbGciOiJIUzI1NiIs...  ?

Response: 200 OK
[
  { "id": 1, "name": "John" },
  { "id": 2, "name": "Jane" }
]
```

## ?? Benefits

### For Developers
1. ? **No Manual Headers** - Set once, use everywhere
2. ? **Persistent** - Survives page refreshes
3. ? **Easy Toggle** - Turn on/off without re-entering
4. ? **Visual Feedback** - Know when auth is active

### For Testing
1. ? **Quick Setup** - Configure in seconds
2. ? **Consistent** - Same header across all endpoints
3. ? **Flexible** - Easy to change tokens
4. ? **Reliable** - No forgotten headers

### For Security
1. ? **Local Only** - Stored in browser, not server
2. ? **Clear State** - Visual indication of configuration
3. ? **Easy Clear** - Disable or clear anytime
4. ? **No Transmission** - Config never sent to server

## ??? Files Modified

**ZeroReflection.ApiGenerator\Resources\swagger.html**

### Changes Made:
1. Added `.btn-config` CSS styles
2. Added configuration button to header
3. Added configuration modal HTML
4. Added `apiConfig` state management
5. Added `loadConfig()` function
6. Added `saveConfig()` function
7. Added `getRequestHeaders()` function
8. Updated `executeEndpoint()` to use configured headers
9. Added localStorage persistence
10. Added visual feedback for active state

## ?? Code Structure

### Configuration Management
```javascript
// State
let apiConfig = { authorizationHeader: '', authEnabled: false };

// Storage
loadConfig()           // Load from localStorage on startup
saveConfigToStorage()  // Save to localStorage

// UI
showConfigModal()      // Open modal
saveConfig()           // Save and close modal
updateConfigButton()   // Update button appearance

// Request Headers
getRequestHeaders()    // Build headers with optional auth
```

### Modal Flow
```
User clicks "?? Configure"
   ?
showConfigModal()
   ?
Load current config into form
   ?
User edits and clicks "?? Save"
   ?
saveConfig()
   ?
Update apiConfig
   ?
Save to localStorage
   ?
Update button appearance
   ?
Close modal
```

## ?? Result

**You can now:**
- ? Set Authorization header once
- ? Use it across all endpoints automatically
- ? Toggle it on/off easily
- ? See visual indication when active
- ? Persist across browser sessions
- ? Test authenticated APIs effortlessly

**The Swagger UI now supports authenticated API testing with persistent configuration!** ????

## ?? Visual Guide

### Header with Config Button
```
??????????????????????????????????????????????????????
? ?? ZeroReflection API Explorer                    ?
? AOT-Compatible API Testing Interface              ?
?                                                    ?
? [?? View Raw JSON]  [?? Configure]  ? New button! ?
??????????????????????????????????????????????????????
```

### Active State (Green)
```
??????????????????????????????????????????????????????
? [?? View Raw JSON]  [?? Configure] ? GREEN! ?    ?
??????????????????????????????????????????????????????
                     (Authorization configured)
```

### Configuration Modal
```
???????????????????????????????????????????????
? ?? API Configuration                   [×] ?
???????????????????????????????????????????????
?                                             ?
? Authorization Header                        ?
? ????????????????????????????????????????????
? ? Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6... ??
? ????????????????????????????????????????????
? This header will be added to all requests  ?
?                                             ?
? ? Enable Authorization Header              ?
?                                             ?
???????????????????????????????????????????????
?              [Cancel]  [?? Save]           ?
???????????????????????????????????????????????
```

**Perfect for testing authenticated APIs!** ??
