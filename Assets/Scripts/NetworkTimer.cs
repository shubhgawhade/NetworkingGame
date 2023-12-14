public class NetworkTimer
{
    public float timer;
    public float MinTimeBetweenTicks { get; }
    public int CurrentTick { get; private set; }

    public NetworkTimer(float serverTickRate, int setTickRate = 0, float setTimer = 0)
    {
        CurrentTick = setTickRate;
        timer = setTimer;
        MinTimeBetweenTicks = 1f / serverTickRate;
    }

    public void Update(float deltaTime) 
    {
        timer += deltaTime;
    }
        
    public bool ShouldTick() 
    {
        if (timer >= MinTimeBetweenTicks) {
            timer -= MinTimeBetweenTicks;
            CurrentTick++;

            if (CurrentTick == 2048)
            {
                CurrentTick = 0;
            }
            return true;
        }

        return false;
    }
}
