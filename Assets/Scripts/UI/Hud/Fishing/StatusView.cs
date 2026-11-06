// StatusView.cs
// Top-right info card (GDD 5.1): time of day and weather, plus the player's currency.
// A "+12" pops up next to the coin counter whenever money is earned.

using UnityEngine;
using UnityEngine.UI;

namespace Tidebook.Ui
{
    public class StatusView : HudView
    {
        GameObject timeRow;
        Image timeIcon;
        Text timeText;
        Image weatherIcon;
        Text weatherText;
        Text coinText;
        Text popText;

        int lastMinute = -1;
        TimeOfDay lastTime = TimeOfDay.Day;
        Weather lastWeather = Weather.Clear;
        bool weatherInit;
        int lastBalance = -1;
        float popAt = -100f;

        protected override void Build(RectTransform layer)
        {
            Image bg = UiBuilder.Box(layer, "StatusCard", UiPalette.WithAlpha(UiPalette.CoverDark, 0.86f), 26);
            UiBuilder.TopRight(bg.rectTransform, 36f, 36f, 340f, 118f);
            UiBuilder.DropShadow(bg, 6f, 0.3f);

            RectTransform row = UiBuilder.Rect(bg.transform, "TimeRow");
            UiBuilder.Stretch(row);
            timeRow = row.gameObject;

            timeIcon = UiBuilder.Icon(row, "TimeIcon", HudSprites.Sun(), Color.white);
            UiBuilder.TopLeft(timeIcon.rectTransform, 18f, 12f, 44f, 44f);
            timeText = UiBuilder.Label(row, "Time", "08:00", 34, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiBuilder.TopLeft(timeText.rectTransform, 70f, 12f, 130f, 44f);
            weatherIcon = UiBuilder.Icon(row, "WeatherIcon", HudSprites.WeatherIcon(Weather.Clear), Color.white);
            UiBuilder.TopLeft(weatherIcon.rectTransform, 206f, 12f, 44f, 44f);
            weatherText = UiBuilder.Label(row, "Weather", "Clear", 20, UiPalette.WaterMid, TextAnchor.MiddleLeft, FontStyle.Bold, true);
            UiBuilder.TopLeft(weatherText.rectTransform, 254f, 12f, 76f, 44f);

            Image coin = UiBuilder.Icon(bg.transform, "Coin", HudSprites.Coin(), Color.white);
            UiBuilder.TopLeft(coin.rectTransform, 18f, 66f, 40f, 40f);
            coinText = UiBuilder.Label(bg.transform, "Coins", "0", 32, UiPalette.Sun, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiBuilder.TopLeft(coinText.rectTransform, 70f, 66f, 160f, 40f);
            popText = UiBuilder.Label(bg.transform, "Pop", "", 28, UiPalette.Mint, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiBuilder.TopLeft(popText.rectTransform, 220f, 66f, 110f, 40f);
        }

        public override void Tick()
        {
            // ---- time + weather ----
            WorldState world = WorldState.Instance;
            timeRow.SetActive(world != null);
            if (world != null)
            {
                int total = (int)(world.hour * 60f);
                if (total != lastMinute)
                {
                    lastMinute = total;
                    timeText.text = (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
                }

                TimeOfDay time = world.CurrentTime;
                if (!weatherInit || time != lastTime)
                {
                    lastTime = time;
                    timeIcon.sprite = time == TimeOfDay.Night ? HudSprites.Moon() : HudSprites.Sun();
                }

                if (!weatherInit || world.weather != lastWeather)
                {
                    lastWeather = world.weather;
                    weatherIcon.sprite = HudSprites.WeatherIcon(world.weather);
                    weatherText.text = FishInfoText.WeatherNoun(world.weather) == "clear skies" ? "Clear"
                        : char.ToUpper(world.weather.ToString()[0]) + world.weather.ToString().Substring(1);
                }
                weatherInit = true;
            }

            // ---- currency ----
            if (ctx.Wallet != null)
            {
                int balance = ctx.Wallet.Balance;
                if (balance != lastBalance)
                {
                    if (lastBalance >= 0 && balance > lastBalance)
                    {
                        popText.text = "+" + (balance - lastBalance);
                        popAt = ctx.Now;
                    }
                    lastBalance = balance;
                    coinText.text = balance.ToString();
                }
            }

            float age = ctx.Now - popAt;
            if (age >= 0f && age < 1.6f)
            {
                float k = age / 1.6f;
                popText.color = UiPalette.WithAlpha(UiPalette.Mint, 1f - k * k);
                popText.rectTransform.anchoredPosition = new Vector2(220f, -66f + k * 14f);
            }
            else
            {
                popText.text = "";
            }
        }
    }
}
