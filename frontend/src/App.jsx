import { useState } from 'react'
import './App.css'

const API_BASE = 'http://localhost:5252'

function App() {
  const [query, setQuery] = useState('')
  const [data, setData] = useState(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)

  async function handleSearch(e) {
    e.preventDefault()
    const name = query.trim()
    if (!name) return

    setLoading(true)
    setError(null)
    setData(null)

    try {
      const res = await fetch(`${API_BASE}/api/country/${encodeURIComponent(name)}`)
      if (res.status === 404) {
        setError(`Country "${name}" not found.`)
        return
      }
      if (!res.ok) {
        setError('Something went wrong. Please try again.')
        return
      }
      setData(await res.json())
    } catch {
      setError('Could not reach the server. Is the backend running?')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="app">
      <header>
        <h1>World Explorer</h1>
        <p>Search for any country to see its info and current weather</p>
      </header>

      <form className="search-form" onSubmit={handleSearch}>
        <input
          type="text"
          placeholder="Enter country name…"
          value={query}
          onChange={e => setQuery(e.target.value)}
          disabled={loading}
        />
        <button type="submit" disabled={loading || !query.trim()}>
          {loading ? 'Searching…' : 'Search'}
        </button>
      </form>

      {loading && <div className="status">Loading…</div>}

      {error && <div className="error">{error}</div>}

      {data && (
        <div className="results">
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

function CountryCard({ country }) {
  return (
    <div className="card country-card">
      <div className="card-header">
        <img src={country.flag} alt={`Flag of ${country.name}`} className="flag" />
        <h2>{country.name}</h2>
      </div>
      <dl className="info-grid">
        <dt>Capital</dt>
        <dd>{country.capital || '—'}</dd>
        <dt>Region</dt>
        <dd>{country.region || '—'}</dd>
        <dt>Population</dt>
        <dd>{country.population.toLocaleString()}</dd>
        <dt>Currency</dt>
        <dd>{country.currency || '—'}</dd>
        <dt>Language</dt>
        <dd>{country.language || '—'}</dd>
      </dl>
    </div>
  )
}

function WeatherCard({ weather }) {
  const iconUrl = `https://openweathermap.org/img/wn/${weather.icon}@2x.png`
  return (
    <div className="card weather-card">
      <h2>Current Weather</h2>
      <div className="weather-main">
        <img src={iconUrl} alt={weather.description} className="weather-icon" />
        <div>
          <div className="temperature">{Math.round(weather.temperature)}°C</div>
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

export default App
