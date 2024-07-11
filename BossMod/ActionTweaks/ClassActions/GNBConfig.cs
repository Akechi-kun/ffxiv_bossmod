namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class GNBConfig : ConfigNode
{
    [PropertyDisplay("Forbid 'Lightning Shot' too early in prepull")]
    public bool ForbidEarlyLightningShot = true;
}
