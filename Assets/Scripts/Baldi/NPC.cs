using UnityEngine;

public class NPC : EntityInstance
{
    public NPCType type;
    public StateMachine<NPCInstance> instance = new StateMachine<NPCInstance>();
    Color[] characters = new Color[]
    {
        //Baldi
        Color.green,
        //Mrs Pomp
         new Color(1f, 0.9499881f, 0f, 1f),
         //Principal
          new Color(0f, 0.1176471f, 0.4823529f, 1f),
          //Crafter
          Color.white,
          //Sweep
          new Color(0f, 0.6226415f, 0.06141377f, 1f),
         //Prize
         new Color(0f, 0.8746934f, 1f, 1f),
         //LookAt
          new Color(0.2588235f, 0.2705882f, 0.1607843f, 1f),
          //Test Enemy
          Color.yellow
    };

    public void Initialize(NPCType typeNew)
    {
        type = typeNew;
        balObject.type = (int)type + 3;
        SetColor(characters[(int)type]);
    }
    public override void GameStart()
    {
        base.GameStart();
        switch (type)
        {
            case NPCType.Baldi:
                break;
            case NPCType.MrsPomp:
                break;
            case NPCType.Principal:
                break;
            case NPCType.Crafter:
                break;
            case NPCType.Sweep:
                break;
            case NPCType.Prize:
                break;
            case NPCType.LookAt:
                break;
            case NPCType.TestEnemy:
                break;
            default:
                break;
        }
        instance.ChangeState(new TestEnemy());
        instance.state.npc = this;
    }
    public override void GameUpdate()
    {
        instance.state.Update();
    }
}

public enum NPCType
{
    Baldi,
    MrsPomp,
    Principal,
    Crafter,
    Sweep,
    Prize,
    LookAt,
    TestEnemy,
    EnumLength
}
public class NPCInstance : StateBase
{
    public NPC npc;
    public StateMachine<NPCState> state = new StateMachine<NPCState>();
}
public class NPCState : StateBase
{

}
public class TestEnemy : NPCInstance
{
    public override void Enter()
    {

    }
    public override void Update()
    {
        state.state.Update();
    }
    public class TestEnemy_Wandering:NPCState
    {
        
    }
}