// WorldCommands.cs
// Dev commands that change the world clock and weather. Receiver: WorldState.
// Both remember what they replaced, so Undo() puts it back.

namespace Tidebook.Dev
{
    public class SetWeatherCommand : ICommand
    {
        readonly WorldState world;
        readonly Weather newValue;
        Weather oldValue;

        public SetWeatherCommand(WorldState world, Weather value)
        {
            this.world = world;
            newValue = value;
            oldValue = world.weather;
        }

        public string Description { get { return "Weather: " + oldValue + " -> " + newValue; } }

        public void Execute()
        {
            oldValue = world.weather;
            world.weather = newValue;
        }

        public void Undo()
        {
            world.weather = oldValue;
        }
    }

    /// <summary>What a SetHourCommand does to the passing of time.</summary>
    public enum TimeFlow { Keep, Hold, Run }

    public class SetHourCommand : ICommand
    {
        readonly WorldState world;
        readonly float newHour;
        readonly TimeFlow flow;
        float oldHour;
        bool oldAdvance;

        public SetHourCommand(WorldState world, float hour, TimeFlow flow)
        {
            this.world = world;
            this.flow = flow;
            newHour = UnityEngine.Mathf.Repeat(hour, 24f);
            oldHour = world.hour;
            oldAdvance = world.advanceTime;
        }

        public string Description
        {
            get
            {
                string text = "Hour: " + oldHour.ToString("0.0") + " -> " + newHour.ToString("0.0");
                if (flow == TimeFlow.Hold) text += " (time held)";
                else if (flow == TimeFlow.Run) text += " (time running)";
                return text;
            }
        }

        public void Execute()
        {
            oldHour = world.hour;
            oldAdvance = world.advanceTime;

            world.hour = newHour;
            if (flow == TimeFlow.Hold) world.advanceTime = false;
            else if (flow == TimeFlow.Run) world.advanceTime = true;
        }

        public void Undo()
        {
            world.hour = oldHour;
            world.advanceTime = oldAdvance;
        }
    }
}
