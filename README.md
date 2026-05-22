> [!NOTE]
> 
> This release has the functionality to rate-limit steam prefill, which the original project lacks.

This repository was my solution to limit the bandwidth of steamprefill and contains two different solutions to the problem.

## Option 1: master branch 

Hard rate limiting functionality from from https://github.com/Yimura/steam-lancache-prefill

This solution sets a hard limit for how fast the prefill downloads files.

The Issue with this solution is that the limit also applies when the prefill continues from a partially downloaded state, so the prefill checks already downloaded files at the same limited speed.

Clone and compile: 
```
git clone --recurse-submodules https://github.com/ledimestari/steam-lancache-prefill.git && \
cd steam-lancache-prefill && \
docker build --no-cache -t steam-prefill-ratelimit .
```

Run with --rate-limit flag, this here is what I used to run the container but change your paths etc accordingly. 
```
docker run -it --rm --name SteamPrefill -e PUID=0 -e PGID=0 --net=host --volume /home/ubuntu/docker/SteamPrefill/config/SteamPrefill:/app/Config steam-prefill-ratelimit prefill --rate-limit 10
```

## Option 2: concurrent-limited branch

This option leverages debug functionality built into https://github.com/tpill90/steam-lancache-prefill

`SteamPrefill/Models/DownloadArguments.cs` contains an option `private int _maxConcurrentRequests = 30;` which was meant for debugging, but by setting this to a low value of 2, I managed to limit the download speed a good amount. 

Benefit over the first option is that the already downloaded files get checked very fast. In my use case the already existing files get checked at a rate of GB/s, while the download itself settles around 20 mbps.

Clone and compile: 
```
git clone --recurse-submodules https://github.com/ledimestari/steam-lancache-prefill.git && \
cd steam-lancache-prefill && \
git checkout concurrent-limited && \
docker build --no-cache -t steam-prefill-ratelimit .
```

Run without any extra flags. 
```
docker run -it --rm --name SteamPrefill -e PUID=0 -e PGID=0 --net=host --volume /home/ubuntu/docker/SteamPrefill/config/SteamPrefill:/app/Config steam-prefill-ratelimit prefill
```

---

Refer README_old.md for the rest of the readme from the original project.
