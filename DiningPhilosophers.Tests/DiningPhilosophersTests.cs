using Xunit;
using Moq;
using PhilosophersStepByStep;
using PhilosophersStepByStep.Strategies;
using PhilosophersStepByStep.Coordinators;
using System.Threading;

namespace PhilosophersStepByStep.Tests
{
    public class DiningPhilosophersTests
    {
        private Mock<IMonitor> CreateMockMonitor()
        {
            return new Mock<IMonitor>();
        }

        // 1. СИМУЛЯЦИЯ
        [Fact]
        public void Philosopher_InitialState_ShouldBeThinking()
        {
            var mockMonitor = CreateMockMonitor();
            var mockStrategy = new Mock<IForkStrategy>();
            var leftFork = new Fork(1, mockMonitor.Object);
            var rightFork = new Fork(2, mockMonitor.Object);

            var philosopher = new Philosopher(1, "Aristotle", leftFork, rightFork, mockStrategy.Object);

            Assert.Equal(PhilosopherState.Thinking, philosopher.State);
        }

        [Fact]
        public void State_Set_ShouldFireStateChangedEvent()
        {
            // Тест проверяет, что при ручном переключении состояния (как это делает метод Run)
            // корректно срабатывает событие, которое нужно для Монитора/UI.
            var mockMonitor = CreateMockMonitor();
            var mockStrategy = new Mock<IForkStrategy>();
            var philosopher = new Philosopher(1, "Plato", new Fork(1, mockMonitor.Object), new Fork(2, mockMonitor.Object), mockStrategy.Object);
            
            // Подписываемся на событие, чтобы проверить его вызов
            bool eventFired = false;
            PhilosopherState? oldState = null;
            PhilosopherState? newState = null;

            philosopher.StateChanged += (sender, args) => 
            {
                eventFired = true;
                oldState = args.PreviousState;
                newState = args.CurrentState;
            };

            // Эмулируем переход от Thinking к Hungry
            philosopher.State = PhilosopherState.Hungry;

            Assert.True(eventFired, "Событие StateChanged должно сработать");
            Assert.Equal(PhilosopherState.Thinking, oldState);
            Assert.Equal(PhilosopherState.Hungry, newState);
            Assert.Equal(PhilosopherState.Hungry, philosopher.State);
        }

        [Fact]
        public void Reset_ShouldReturnPhilosopherToThinkingState()
        {
            var mockMonitor = CreateMockMonitor();
            var mockStrategy = new Mock<IForkStrategy>();
            var leftFork = new Fork(1, mockMonitor.Object);
            var rightFork = new Fork(2, mockMonitor.Object);

            var philosopher = new Philosopher(1, "Socrates", leftFork, rightFork, mockStrategy.Object);
            
            philosopher.State = PhilosopherState.Eating;
            
            philosopher.Reset();
            Assert.Equal(PhilosopherState.Thinking, philosopher.State);
        }

        [Fact]
        public void TryAcquireForks_ShouldReturnTrue_WhenForksAreFree()
        {
            var mockMonitor = CreateMockMonitor();
            var mockStrategy = new Mock<IForkStrategy>();
            
            var leftFork = new Fork(1, mockMonitor.Object);
            var rightFork = new Fork(2, mockMonitor.Object);

            // Настраиваем мок стратегии: она говорит "бери левую, потом правую"
            mockStrategy.Setup(s => s.GetFirstFork(It.IsAny<int>(), leftFork, rightFork, false, false))
                        .Returns(leftFork);
            mockStrategy.Setup(s => s.GetSecondFork(It.IsAny<int>(), leftFork, rightFork, leftFork))
                        .Returns(rightFork);

            var philosopher = new Philosopher(1, "Plato", leftFork, rightFork, mockStrategy.Object);

            bool result = philosopher.TryAcquireForks();
            Assert.True(result, "Философ должен успешно взять свободные вилки");
            Assert.Equal(ForkState.InUse, leftFork.State);
            Assert.Equal(ForkState.InUse, rightFork.State);
            Assert.Equal(philosopher, leftFork.Holder);
        }

        [Fact]
        public void TryAcquireForks_ShouldReturnFalse_WhenFirstForkIsTaken()
        {
            var mockMonitor = CreateMockMonitor();
            var mockStrategy = new Mock<IForkStrategy>();
            var leftFork = new Fork(1, mockMonitor.Object);
            var rightFork = new Fork(2, mockMonitor.Object);

            // Имитируем, что левая вилка занята другим философом
            var otherGuy = new Philosopher(99, "Other", leftFork, rightFork, mockStrategy.Object);
            leftFork.TryPickUp(otherGuy); 

            // Стратегия все равно предлагает попробовать взять левую
            mockStrategy.Setup(s => s.GetFirstFork(It.IsAny<int>(), leftFork, rightFork, true, false))
                        .Returns(leftFork);

            var philosopher = new Philosopher(1, "Socrates", leftFork, rightFork, mockStrategy.Object);

            bool result = philosopher.TryAcquireForks();
            Assert.False(result, "Философ не должен взять вилки, если первая занята");
            Assert.Null(philosopher.HeldFork1); // У философа в руках ничего нет
        }
        
        [Fact]
        public void ReleaseForks_ShouldMakeForksAvailable()
        {
            var mockMonitor = CreateMockMonitor();
            var mockStrategy = new Mock<IForkStrategy>();
            var leftFork = new Fork(1, mockMonitor.Object);
            var rightFork = new Fork(2, mockMonitor.Object);

            mockStrategy.Setup(s => s.GetFirstFork(It.IsAny<int>(), leftFork, rightFork, false, false)).Returns(leftFork);
            mockStrategy.Setup(s => s.GetSecondFork(It.IsAny<int>(), leftFork, rightFork, leftFork)).Returns(rightFork);

            var philosopher = new Philosopher(1, "Kant", leftFork, rightFork, mockStrategy.Object);
            philosopher.TryAcquireForks(); 

            philosopher.ReleaseForks();
            Assert.Equal(ForkState.Available, leftFork.State);
            Assert.Equal(ForkState.Available, rightFork.State);
            Assert.Null(leftFork.Holder);
        }

        // 2. СТРАТЕГИИ
        [Fact]
        public void OrderedStrategy_ShouldAlwaysPickLeftForkFirst()
        {
            var strategy = new OrderedForkStrategy(); 
            var leftFork = new object();
            var rightFork = new object();

            var firstChoice = strategy.GetFirstFork(0, leftFork, rightFork, false, false);
            Assert.Equal(leftFork, firstChoice);
        }

        [Fact]
        public void HierarchyStrategy_ShouldPickLowerIdForkFirst()
        {
            var strategy = new HierarchyForkStrategy();
            var monitor = CreateMockMonitor().Object;
            
            var fork1 = new Fork(1, monitor); // Меньший ID
            var fork5 = new Fork(5, monitor); // Больший ID

            var result1 = strategy.GetFirstFork(1, fork1, fork5, false, false);
            Assert.Equal(fork1, result1);

            // Если fork1 справа, все равно должны выбрать fork1 (так как ID меньше)
            var result2 = strategy.GetFirstFork(1, fork5, fork1, false, false);
            Assert.Equal(fork1, result2);
        }

        // 3. DEADLOCK
        [Fact]
        public void DetectDeadlock_ShouldReturnTrue_WhenAllPhilosophersAreHungryAndHoldingOneFork()
        {
            var mockMonitor = CreateMockMonitor();
            var mockStrategy = new Mock<IForkStrategy>();

            // Вечно заблокированная вилка
            var dummyLockedFork = new Fork(999, mockMonitor.Object);
            dummyLockedFork.TryPickUp(new Philosopher(999, "Blocker", dummyLockedFork, dummyLockedFork, mockStrategy.Object));

            // Стратегия дедлока. Первая вилка всегда перется успешно, а вторая всегда занята, философы никогда не отпускают вилки при неудаче захвата второй
            mockStrategy.Setup(s => s.GetFirstFork(It.IsAny<int>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<bool>(), It.IsAny<bool>()))
                        .Returns((int id, object l, object r, bool lb, bool rb) => l);
            mockStrategy.Setup(s => s.GetSecondFork(It.IsAny<int>(), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<object>()))
                        .Returns(dummyLockedFork);
            mockStrategy.Setup(s => s.ShouldReleaseFirstFork(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                        .Returns(false); 

            var config = new PhilosopherConfiguration 
            { 
                PhilosopherCount = 2, 
                PhilosopherNames = new List<string> { "Plato", "Socrates" } 
            };
            
            var table = new Table(mockMonitor.Object, config, mockStrategy.Object);
            var metricsCalculator = new MetricsCalculator(table);

            foreach (var philosopher in table.Philosophers)
            {
                philosopher.State = PhilosopherState.Hungry;
                philosopher.TryAcquireForks();
            }

            bool isDeadlock = metricsCalculator.DetectDeadlock();

            Assert.True(isDeadlock, "Метрики должны обнаружить Deadlock, когда все философы голодны и держат вилку");
            Assert.All(table.Philosophers, p => Assert.NotNull(p.HeldFork1));
            Assert.All(table.Philosophers, p => Assert.Null(p.HeldFork2));
        }

    }
    
    public class HierarchyForkStrategy : IForkStrategy
    {
        public object GetFirstFork(int philosopherId, object leftFork, object rightFork, bool isLeftBlocked, bool isRightBlocked)
        {
            var l = (Fork)leftFork;
            var r = (Fork)rightFork;
            return l.Id < r.Id ? l : r;
        }

        public object GetSecondFork(int philosopherId, object leftFork, object rightFork, object heldFork)
        {
            var l = (Fork)leftFork;
            var r = (Fork)rightFork;
            return heldFork == l ? r : l;
        }

        public int GetMaxAttempts(int philosopherId) => 1;
        public bool ShouldReleaseFirstFork(int philosopherId, int attemptsToGetSecondFork, int maxAttempts) => true;
    }
}