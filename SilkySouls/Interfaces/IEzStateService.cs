// 

using static SilkySouls.GameIds.EzState;

namespace SilkySouls.Interfaces;

public interface IEzStateService
{
    void ExecuteTalkCommand(TalkCommand command);
}