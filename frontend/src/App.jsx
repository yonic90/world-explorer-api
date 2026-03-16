// App.jsx — Root component and all child components
//
// React components are just functions that return JSX (HTML-like syntax).
// This file contains three components, kept together since the app is small:
//   - App         → manages state and data fetching (the "smart" component)
//   - CountryCard → displays country information (a "dumb" / presentational component)
//   - WeatherCard → displays weather information  (a "dumb" / presentational component)

import { useState } from 'react'
import './App.css'

// Base URL of the ASP.NET Core backend.
// Defined as a constant at the top so it's easy to change for different environments.
const API_BASE = 'http://localhost:5252'

// ============================================================
// App — root component
//
// Holds all state for the page and coordinates the search flow:
//   1. User types a country name and submits the form
//   2. App fetches from the backend API
//   3. App passes the response data down to CountryCard and WeatherCard
// ============================================================
function App() {
  // useState returns [currentValue, setterFunction].
  // Calling the setter re-renders the component with the new value.

  const [query, setQuery] = useState('')      // Current text in the search input
  const [data, setData] = useState(null)      // API response: { country, weather }
  const [loading, setLoading] = useState(false) // True while the fetch is in progress
  const [error, setError] = useState(null)    // Error message string, or null

  // handleSearch is called when the form is submitted.
  // Marked async so we can use await inside it.
  async function handleSearch(e) {
    // e.preventDefault() stops the browser's default form behaviour,
    // which would reload the page on submit.
    e.preventDefault()

    const name = query.trim() // Remove leading/trailing whitespace
    if (!name) return         // Do nothing if the input is empty

    // Reset state before each new search so stale results don't show.
    setLoading(true)
    setError(null)
    setData(null)

    try {
      // encodeURIComponent makes the country name URL-safe.
      // e.g. "New Zealand" → "New%20Zealand"
      const res = await fetch(`${API_BASE}/api/country/${encodeURIComponent(name)}`)

      // Handle specific HTTP status codes before reading the body.
      if (res.status === 404) {
        setError(`Country "${name}" not found.`)
        return
      }
      if (!res.ok) {
        // res.ok is true for 200–299 status codes.
        setError('Something went wrong. Please try again.')
        return
      }

      // res.json() parses the response body as JSON.
      // The shape matches the backend: { country: CountryInfo, weather: WeatherInfo | null }
      setData(await res.json())
    } catch {
      // fetch() throws only on network errors (server unreachable, DNS failure, etc.),
      // NOT on 4xx/5xx responses — those are handled above with res.ok.
      setError('Could not reach the server. Is the backend running?')
    } finally {
      // finally always runs — whether the try succeeded or the catch fired.
      // Used here to always clear the loading state.
      setLoading(false)
    }
  }

  // JSX looks like HTML but it's compiled to JavaScript function calls.
  // Curly braces {} let you embed any JavaScript expression inline.
  // Conditional rendering uses short-circuit evaluation: {condition && <Element />}
  return (
    <div className="app">
      {/* className is used instead of class because "class" is a reserved word in JS */}
      <header>
        <h1>World Explorer</h1>
        <p>Search for any country to see its info and current weather</p>
      </header>

      {/* Controlled form: React owns the input value via the query state.
          onSubmit fires when the user presses Enter or clicks the button. */}
      <form className="search-form" onSubmit={handleSearch}>
        <input
          type="text"
          placeholder="Enter country name…"
          value={query}                          // Controlled: value comes from state
          onChange={e => setQuery(e.target.value)} // Update state on every keystroke
          disabled={loading}                     // Prevent typing while fetching
        />
        <button type="submit" disabled={loading || !query.trim()}>
          {/* Ternary expression to switch button label during loading */}
          {loading ? 'Searching…' : 'Search'}
        </button>
      </form>

      {/* Show a loading message while the fetch is in progress */}
      {loading && <div className="status">Loading…</div>}

      {/* Show the error message if one exists */}
      {error && <div className="error">{error}</div>}

      {/* Show results only when data has been loaded.
          data.weather can be null (if OpenWeatherMap key is missing),
          so we show a fallback card in that case. */}
      {data && (
        <div className="results">
          {/* Props are passed to child components like HTML attributes.
              The child receives them as a single "props" object — here we
              destructure directly: ({ country }) and ({ weather }). */}
          <CountryCard country={data.country} />
          {data.weather ? (
            <WeatherCard weather={data.weather} />
          ) : (
            <div className="card weather-card no-weather">
              <p>Weather data unavailable</p>
            </div>
          )}
        </div>
      )}
    </div>
  )
}

// ============================================================
// CountryCard — presentational component
//
// Receives a single prop (country) and renders it as a card.
// No state, no side effects — just data in, UI out.
// ============================================================
function CountryCard({ country }) {
  return (
    <div className="card country-card">
      <div className="card-header">
        {/* country.flag is a URL to the flag PNG, provided by the RestCountries API */}
        <img src={country.flag} alt={`Flag of ${country.name}`} className="flag" />
        <h2>{country.name}</h2>
      </div>

      {/* <dl> is a description list — semantically correct for label/value pairs.
          <dt> = description term (the label)
          <dd> = description detail (the value)
          The || '—' fallback shows an em dash when a field is empty. */}
      <dl className="info-grid">
        <dt>Capital</dt>
        <dd>{country.capital || '—'}</dd>
        <dt>Region</dt>
        <dd>{country.region || '—'}</dd>
        <dt>Population</dt>
        {/* toLocaleString() formats the number with commas: 1234567 → "1,234,567" */}
        <dd>{country.population.toLocaleString()}</dd>
        <dt>Currency</dt>
        <dd>{country.currency || '—'}</dd>
        <dt>Language</dt>
        <dd>{country.language || '—'}</dd>
      </dl>
    </div>
  )
}

// ============================================================
// WeatherCard — presentational component
//
// Receives a single prop (weather) and renders the current conditions.
// ============================================================
function WeatherCard({ weather }) {
  // Build the icon image URL from the icon code returned by OpenWeatherMap.
  // Icon codes look like "01d" (clear sky, day) or "10n" (rain, night).
  // @2x gives us the higher-resolution (retina) version.
  const iconUrl = `https://openweathermap.org/img/wn/${weather.icon}@2x.png`

  return (
    <div className="card weather-card">
      <h2>Current Weather</h2>

      <div className="weather-main">
        <img src={iconUrl} alt={weather.description} className="weather-icon" />
        <div>
          {/* Math.round() removes the decimal from the temperature float.
              e.g. 22.7 → 23 */}
          <div className="temperature">{Math.round(weather.temperature)}°C</div>
          {/* description comes as lowercase from the API, e.g. "light rain"
              CSS text-transform: capitalize handles the visual capitalisation */}
          <div className="description">{weather.description}</div>
        </div>
      </div>

      <dl className="info-grid">
        <dt>Feels like</dt>
        <dd>{Math.round(weather.feelsLike)}°C</dd>
        <dt>Humidity</dt>
        <dd>{weather.humidity}%</dd>
      </dl>
    </div>
  )
}

// Export App as the default export so main.jsx can import it.
// Named exports (export function Foo) and default exports (export default Foo)
// are both valid — default is conventional for the main component of a file.
export default App
