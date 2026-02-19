# JCO Previous Day, Week & Month High Low Indicator

A cTrader indicator that displays Previous Day, Week, and Month High/Low levels, along with the NY Midnight opening price line.

![Indicator Screenshot](screenshot.png)

## Features

- **Previous Day Levels**: High, Low, and Mid (50%) lines with customizable colors
- **Previous Week Levels**: High and Low lines
- **Previous Month Levels**: High and Low lines (purple by default)
- **NY Midnight Line**: Opening price at New York midnight (00:00 EST)
- **Labels**: Configurable labels on all lines (PDH, PDM, PDL, PWH, PWL, PMH, PML, 0 NY)
- **Dashboard**: Price ranges and levels displayed on the chart
- **Visibility Toggles**: Show/hide each group independently
- **Vertical Lines**: Optional markers for day/week/month boundaries

## Parameters

### Previous Day
| Parameter | Default | Description |
|-----------|---------|-------------|
| Show Day Lines | true | Toggle visibility of day lines |
| Starting Hour | 0 | Hour (UTC) to start the day calculation |
| High Line Color | Green | Color for the high line |
| Mid Line Color | Gray | Color for the mid line |
| Low Line Color | Green | Color for the low line |
| Show Mid Line | true | Toggle mid line visibility |
| Line Thickness | 2 | Thickness of the lines |
| Extend Lines (Candles) | 10 | Number of candles to extend lines |
| Vertical Line Color | Blue | Color for vertical day markers |
| Label Font Size | 9 | Font size for labels |

### Previous Week
| Parameter | Default | Description |
|-----------|---------|-------------|
| Show Week Lines | true | Toggle visibility of week lines |
| High Line Color | Orange | Color for the high line |
| Low Line Color | Orange | Color for the low line |
| Line Thickness | 2 | Thickness of the lines |
| Extend Lines (Candles) | 10 | Number of candles to extend lines |
| Vertical Line Color | DarkOrange | Color for vertical week markers |
| Label Font Size | 9 | Font size for labels |

### Previous Month
| Parameter | Default | Description |
|-----------|---------|-------------|
| Show Month Lines | true | Toggle visibility of month lines |
| High Line Color | Purple | Color for the high line |
| Low Line Color | Purple | Color for the low line |
| Line Thickness | 2 | Thickness of the lines |
| Extend Lines (Candles) | 10 | Number of candles to extend lines |
| Vertical Line Color | DarkViolet | Color for vertical month markers |
| Label Font Size | 9 | Font size for labels |

### NY Midnight
| Parameter | Default | Description |
|-----------|---------|-------------|
| Show NY Midnight Line | true | Toggle visibility of midnight line |
| Line Color | DodgerBlue | Color for the line |
| Line Thickness | 2 | Thickness of the line |
| Line Style | DotsRare | Style of the line |
| Extend Lines (Candles) | 10 | Number of candles to extend line |
| Label Font Size | 9 | Font size for label |

### Display
| Parameter | Default | Description |
|-----------|---------|-------------|
| Show Vertical Lines | true | Toggle vertical boundary markers |
| Display Dashboard | true | Toggle price dashboard |
| Enable Print | false | Enable debug output |

## Installation

1. Download the `.cs` file
2. Open cTrader
3. Go to **Automate** > **Indicators**
4. Click **New Indicator** or import the file
5. Build and add to your chart

## Changelog

### v2.4 (2026-02-19)
- Added Previous Month High & Low lines (purple by default)
- PMH / PML labels and vertical markers for the month boundaries
- Month range added to the dashboard
- Month calculation uses daily bars for optimal performance

### v2.3 (2026-01-20)
- Added NY Midnight opening price line
- Added labels on all lines (PDH, PDM, PDL, PWH, PWL, 0 NY)
- Added configurable font size for labels per group

### v2.1 (2025-01-14)
- Added visibility toggles for day lines, week lines, and vertical lines
- Dashboard now always displays values independently of line visibility

### v2.0 (2025-01-14)
- Added Previous Week High & Low levels
- Reorganized parameters by section

### v1.0
- Previous Day High/Low with mid line
- Configurable colors and display options

## License

MIT License

## Author

J. Cornier

## Links

- [GitHub Repository](https://github.com/jcornierfra/cTrader-Indicator-JCO-Previous-Day-Week-HL)
