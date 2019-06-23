using NullD.Net.GS.Message.Definitions.Animation;
using NullD.Core.GS.Common.Types.Math;
using NullD.Common.Logging;
using System.Threading.Tasks;
using NullD.Net.GS.Message;
using System.Collections.Generic;
using System;

namespace NullD.Core.GS.QuestEvents.Implementations
{
    class _167115 : QuestEvent
    {
        private static readonly Logger Logger = LogManager.CreateLogger();

        public _167115()
            : base(167115)
        {
        }

        static int RiseZombieAID = 6644;
        static int CapitanDaltynAID = 156801; //Actor ID Капитана Далтина
        List<uint> CapitanDaltynKiller = new List<uint> { }; // Используем для отслеживания убийства


        public override void Execute(Map.World world)
        {

            var boss = world.GetActorBySNO(CapitanDaltynAID);
            var minions = world.GetActorsBySNO(RiseZombieAID);
            if (boss == null)
            {
                Logger.Debug("Не найдено: Капитан Далтин - {0}", CapitanDaltynAID);
                Vector3D CapitanDaltyn = new Vector3D(51.12595f, 100.2664f, 0.1000305f);
                Vector3D[] Zombies = new Vector3D[4];
                Zombies[0] = new Vector3D(50.00065f, 125.4087f, 0.1000305f);
                Zombies[1] = new Vector3D(54.88688f, 62.24541f, 0.1000305f);
                Zombies[2] = new Vector3D(86.45869f, 77.09571f, 0.1000305f);
                Zombies[3] = new Vector3D(102.117f, 97.59058f, 0.1000305f);
                var Daltyn = world.SpawnMonsterWithGet(CapitanDaltynAID, CapitanDaltyn);
                //[011523] [Anim] zombie_male_skinny_spawn
                world.BroadcastIfRevealed(new PlayAnimationMessage
                {
                    ActorID = Daltyn.DynamicID,
                    Field1 = 5,
                    Field2 = 0,
                    tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                            {
                        new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                        {
                            Duration = 20,
                            AnimationSNO = 11523,
                            PermutationIndex = 0,
                            Speed = 0.9f
                        }
                            }
                }, Daltyn);
                (Daltyn as Actors.Monster).Brain.DeActivate();
                foreach (Vector3D point in Zombies)
                {
                    var Zombie = world.SpawnMonsterWithGet(RiseZombieAID, point);
                    CapitanDaltynKiller.Add(Zombie.DynamicID);

                    Zombie.Attributes[GameAttribute.Quest_Monster] = true;
                    (Zombie as Actors.Monster).Brain.DeActivate();
                    world.BroadcastIfRevealed(new PlayAnimationMessage
                    {
                        ActorID = Zombie.DynamicID,
                        Field1 = 5,
                        Field2 = 0,
                        tAnim = new Net.GS.Message.Fields.PlayAnimationMessageSpec[]
                           {
                        new Net.GS.Message.Fields.PlayAnimationMessageSpec()
                        {
                            Duration = 160,
                            AnimationSNO = 11523,
                            PermutationIndex = 0,
                            Speed = 1f
                        }
                           }
                    }, Zombie);
                }
                boss = world.GetActorBySNO(CapitanDaltynAID);
                CapitanDaltynKiller.Add(boss.DynamicID);
                minions = world.GetActorsBySNO(RiseZombieAID);
                boss.RunSpeed = 0.33f;
                boss.WalkSpeed = 0.33f;

                Ticker.TickTimer Timeout = new Ticker.SecondsTickTimer(world.Game, 3.5f);
                var Boom = System.Threading.Tasks.Task<bool>.Factory.StartNew(() => WaitToSpawn(Timeout));
                Boom.ContinueWith(delegate
                {
                    (Daltyn as Actors.Monster).Brain.Activate();
                    foreach (var minion in minions)
                    {
                        (minion as Actors.Monster).Brain.Activate();
                    }
                });

            }
            else
            {
                CapitanDaltynKiller.Add(boss.DynamicID);
            }

            // Пытаемся привязать статус босса!
            boss = world.GetActorBySNO(CapitanDaltynAID);
            boss.Attributes[Net.GS.Message.GameAttribute.Using_Bossbar] = true;
            boss.Attributes[Net.GS.Message.GameAttribute.InBossEncounter] = true;
            // DOES NOT WORK it should be champion affixes or shit of this kind ...
            // Увеличиваем здоровье босса!

            boss.Attributes[GameAttribute.Hitpoints_Max] = 200f;
            boss.Attributes[GameAttribute.Hitpoints_Cur] = 200f;
            boss.Attributes[GameAttribute.Movement_Scalar_Reduction_Percent] -= 10f;
            boss.Attributes[GameAttribute.Quest_Monster] = true;


            //Запуск отслеживания убийства
            var ListenerDaltyn = Task<bool>.Factory.StartNew(() => OnKillListener(CapitanDaltynKiller, world));
            //Ждём пока убьют
            ListenerDaltyn.ContinueWith(delegate
            {
                //{[Actor] [Type: Monster] SNOId:203030 DynamicId: 3634 Position: x:149,8516 y:60,33301 z:3,051758E-05 Name: Leah_AdriaCellar}
                var Leah_Cellar = world.GetActorBySNO(203030);
                Leah_Cellar.Position = new Vector3D(149.8516f, 60.33301f, 3.051758E-05f);
                Leah_Cellar.Attributes[Net.GS.Message.GameAttribute.MinimapActive] = true;
                (Leah_Cellar as Actors.InteractiveNPC).Conversations.Clear();
                (Leah_Cellar as Actors.InteractiveNPC).Conversations.Add(new Actors.Interactions.ConversationInteraction(198588));
                Leah_Cellar.Attributes[Net.GS.Message.GameAttribute.Conversation_Icon, 0] = 1;
                Leah_Cellar.Attributes.BroadcastChangedIfRevealed();


                foreach (var player in world.Players)
                {
                    try
                    {
                        if (player.Value.PlayerIndex == 0)
                        {
                            world.Game.Quests.Advance(72095);
                        }
                        if (player.Value.ActiveHireling != null)
                        {
                            player.Value.ActiveHireling = null;
                            player.Value.ActiveHirelingProxy = null;
                        }
                    }
                    catch { }
                }
            });

        }

        private bool WaitToSpawn(Ticker.TickTimer timer)
        {
            while (timer.TimedOut != true)
            {

            }
            return true;
        }

        private bool StartConversation(Map.World world, Int32 conversationId)
        {
            foreach (var player in world.Players)
            {
                player.Value.Conversations.StartConversation(conversationId);
            }
            return true;
        }

        private bool OnKillListener(List<uint> monstersAlive, Map.World world)
        {
            Int32 monstersKilled = 0;
            var monsterCount = monstersAlive.Count;
            while (monstersKilled != monsterCount)
            {
                for (int i = monstersAlive.Count - 1; i >= 0; i--)
                {
                    if (world.HasMonster(monstersAlive[i]))
                    {
                        //Alive: Nothing.
                    }
                    else
                    {
                        //If dead we remove it from the list and keep iterating.
                        Logger.Debug(monstersAlive[i] + " has been killed");
                        monstersAlive.RemoveAt(i);
                        monstersKilled++;
                    }
                }
            }
            return true;
        }
    }
}
