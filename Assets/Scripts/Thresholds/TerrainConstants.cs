namespace Thresholds
{
    public abstract class TerrainConstants
    {
        // HeightType
        public const float DeepWaterTr = 0.25f;
        public const float MidWaterTr = 0.3f;
        public const float ShallowWaterTr = 0.34f;
        
        public const float MountainTr = 0.675f;
        public const float HillTr = 0f;
        
        // Prec. Type
        public const float StormyTr = 0.5f;
        public const float RainyTr = 0.2f;
        public const float MildRainTr = 0f;
        public const float RarelyRainTr = -0.2f;
        public const float DryTr = -0.5f;
        
        // Temp. Type
        public const float VeryHotTempTr = 0.85f;
        public const float HotTempTr = 0.65f;
        public const float MildTempTr = 0.25f;
        public const float ColdTempTr = 0.12f;
        public const float VeryColdTempTr = 0.00f;

    }
}
