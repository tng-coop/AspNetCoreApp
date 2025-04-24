#!/usr/bin/env bash
set -e

# 0) Ensure ImageMagick and ffmpeg are installed
command -v convert >/dev/null 2>&1 || { echo >&2 "Install ImageMagick."; exit 1; }
command -v ffmpeg  >/dev/null 2>&1 || { echo >&2 "Install ffmpeg.";     exit 1; }

# 1) Prepare directories
mkdir -p images videos

# 2) Font choices
BASE_FONT="DejaVu-Sans"
JP_FONT="IPAMincho"    # IPA 明朝; covers kanji

# 3) Define per-frame labels for each of the 10 videos
labels1=(1 2 3 4 5 6 7 8 9 10)
labels2=(I II III IV V VI VII VIII IX X)
labels3=(one two three four five six seven eight nine ten)
labels4=(1 10 11 100 101 110 111 1000 1001 1010)        # binary for 1–10
labels5=(\| \|\| \|\|\| \|\|\|\| \|\|\|\|\| \|\|\|\|\|\| \|\|\|\|\|\|\| \|\|\|\|\|\|\|\| \|\|\|\|\|\|\|\|\| \|\|\|\|\|\|\|\|\|\|)
labels6=(一 二 三 四 五 六 七 八 九 十)                 # kanji for 1–10
labels7=(.---- ..--- ...-- ....- ..... -.... --... ---.. ----. ".---- -----")  # Morse for 1–10
labels8=("*" "**" "***" "****" "*****" "******" "*******" "********" "*********" "**********")
labels9=(1️⃣ 2️⃣ 3️⃣ 4️⃣ 5️⃣ 6️⃣ 7️⃣ 8️⃣ 9️⃣ 🔟)        # emoji digits
labels10=(uno dos tres cuatro cinco seis siete ocho nueve diez)

# 4) Loop through videos 1–10
for i in {1..10}; do
  declare -n arr="labels${i}"
  font="$BASE_FONT"
  [[ "$i" -eq 6 ]] && font="$JP_FONT"

  # Generate 10 unique frames
  for ((j=0; j<10; j++)); do
    txt="${arr[j]}"
    (( ${#txt} <= 2 )) && ps=400 || ps=200

    convert -size 640x480 xc:white \
            -gravity center \
            -font "$font" \
            -pointsize "$ps" \
            -fill black \
            -annotate +0+0 "$txt" \
            "images/video${i}_frame_$((j+1)).png"
  done

  # Pack into a 1 s MP4 at 10 fps (→ exactly 10 frames)
  ffmpeg -y -framerate 10 \
         -i images/video${i}_frame_%d.png \
         -c:v libx264 -pix_fmt yuv420p \
         "images/video${i}.mp4"

  # Clean up frames
  rm images/video${i}_frame_*.png
done

echo "All done! Your videos (video1.mp4 … video10.mp4) are in the videos/ folder."
