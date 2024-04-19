import astropy.units as u
from astropy.coordinates import SkyCoord, Galactic
from astroquery.gaia import Gaia
import numpy as np
from astropy.table import Table
import csv

stars = []

def calculate_color(bp, rp):
    """Calculate the BP-RP color index."""
    return bp - rp

def color_index_to_rgb(color_index):
    """Map BP-RP color index to an approximate RGB value."""
    if color_index < 0.75:
        return "(0, 0, 255)"  # Blue
    elif color_index < 1.5:
        return "(255,255,255)"  # White
    elif color_index < 2.5:
        return "(255,255,0)"  # Yellow
    else:
        return "(255, 0, 0)"  # Red

def calculate_luminosity(g, parallax):
    """Calculate luminosity based on G magnitude and parallax."""
    if parallax < 0:
        parallax = -parallax
    if parallax == 0:
        return None  # Avoid division by zero or negative parallax values
    distance_pc = 1 / (parallax / 1000)  # Convert parallax from mas to arcsec, then calculate distance in parsecs
    M = g - 5 * (np.log10(distance_pc) - 1)  # Calculate absolute magnitude
    L = 10**(0.4 * (4.83 - M))  # Luminosity in terms of the Sun's luminosity
    return L

def celestial_to_cartesian(ra_deg, dec_deg, distance):
    # Convert RA and Dec from degrees to radians
    ra_rad = np.radians(ra_deg)
    dec_rad = np.radians(dec_deg)
    
    # Calculate Cartesian coordinates
    x = np.cos(dec_rad) * np.cos(ra_rad) * distance
    y = np.cos(dec_rad) * np.sin(ra_rad) * distance
    z = np.sin(dec_rad) * distance
    
    return "(" + str(x) + "," + str(y) + "," + str(z) + ")"

def add_star():
    # Generate random coordinates
    ra_random = np.random.uniform(0, 360) * u.degree  # RA between 0 and 360 degrees
    dec_random = np.random.uniform(-90, 90) * u.degree  # Dec between -90 and +90 degrees

    # Create a SkyCoord object with random coordinates
    coord_random = SkyCoord(ra=ra_random, dec=dec_random, unit=(u.degree, u.degree), frame='icrs')
    print(f"Random coordinates: {coord_random}")

    # Define search area
    width = u.Quantity(0.1, u.deg)
    height = u.Quantity(0.1, u.deg)

    # Query Gaia database for stars around the random coordinates
    all_result = Gaia.query_object_async(coordinate=coord_random, width=width, height=height)
    curIndex = 0
    if len(all_result) <= 0:
        return
    r = all_result[curIndex]
    bp = r['phot_bp_mean_mag']
    rp = r['phot_rp_mean_mag']
    g = r['phot_g_mean_mag']
    parallax = r['parallax']
    cartesian_coord = celestial_to_cartesian(r['ra'], r['dec'], r['dist'] * 10000)

    color_index = calculate_color(bp, rp)
    color = color_index_to_rgb(color_index)
    luminosity = calculate_luminosity(g, parallax)
    if not luminosity:
        luminosity = 1.0

    stars.append([cartesian_coord, color, luminosity])

for i in range(200):
    add_star()

# Specify the filename
filename = 'star_data.csv'

# Open the file with write permission ('w')
with open(filename, 'w', newline='') as csvfile:
    # Create a CSV writer object
    csvwriter = csv.writer(csvfile, delimiter=';')
    
    # Write the header row
    csvwriter.writerow(['Coordinate', 'Color(R,G,B)', 'Luminosity(L/L_sun)'])
    
    # Write the data row
    for star in stars:
        csvwriter.writerow(star)

print(f"Data written to {filename}")
