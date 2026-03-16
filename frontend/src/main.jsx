// main.jsx — Application entry point
//
// This is the first JavaScript file that runs. Its only job is to mount
// the React application into the HTML page.

import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'   // Global baseline styles (reset, font smoothing)
import App from './App.jsx'

// document.getElementById('root') finds the <div id="root"> in index.html.
// createRoot() tells React to take control of that element.
// All React-rendered content will live inside it.
createRoot(document.getElementById('root')).render(
  // StrictMode is a development-only wrapper — it renders components twice
  // to help catch side-effects and highlight potential problems.
  // It has no effect in the production build.
  <StrictMode>
    <App />
  </StrictMode>,
)
