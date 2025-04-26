import requests
import os

# Replace with your personal access token
try:
    TOKEN = os.environ["Vimeo__Token"]
except KeyError:
    raise RuntimeError("The environment variable Vimeo__Token is not set")

headers = {"Authorization": f"Bearer {TOKEN}"}

# Request only the URI, name, and page link for each video
params = {
    "per_page": 50,
    "fields": "uri,name,link"
}

# Fetch your videos
resp = requests.get("https://api.vimeo.com/me/videos", headers=headers, params=params)
resp.raise_for_status()
data = resp.json()

# Print total and generate SetNameAsync calls
print(f"Total videos: {data['total']}")
for v in data["data"]:
    # Escape any quotes in the video name
    name = v["name"].replace('"', '\\"')
    link = v["link"]
    print(f'await nameSvc.SetNameAsync("{name}", "{link}", ownerId: null);')
