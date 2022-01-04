# WaveletDatabase
Experimental time-series aggregation database

## Concept
Data series are stored in a pyramid structure governed by a pair of aggregation functions, acting as a wavelet set.
Higher-resolution layers of each series can be limited in size, meaning that older data naturally limits its data size by reducing resolution over time.

Individual events are bucketed at the bottom of the pyramid, getting abandoned as soon as the best resolution timescale expires.
