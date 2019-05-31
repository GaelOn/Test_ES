using System;
using Domain.Mock.Implem;
using Domain.Base.Aggregate;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;

namespace Domain.Base.Test.AggregateTest
{
    [TestFixture]
    public class EntityListTest
    {
        private EntityList<int, int> _entityList;

        [SetUp]
        public void Initialize() => _entityList = new EntityList<int, int>();

        [Test]
        public void EntityList_CanAdd_Entity_WithOut_Throwing()
        {
            // Arrange
            var entity = new FirstSubProcess("test", 1, DateTime.Now);
            //Act
            Action addAction = () => _entityList.Add(entity);
            //Assert
            addAction.Should().NotThrow();
        }

        [Test]
        public void EntityList_Can_Provide_Count_Information()
        {
            // Arrange
            var expectedCountValue = 4;
            InitializeListOfFirsSubProcessEntity(expectedCountValue, DateTime.Now);
            //Act
            var countValue = _entityList.Count;
            //Assert
            countValue.Should().Be(expectedCountValue);
        }

        [Test]
        public void Entity_Can_Be_Retrieved_By_Id_Entity_List()
        {
            // Arrange
            InitializeListOfFirsSubProcessEntity(10, DateTime.Now);
            var idToFind = 4;
            //Act
            var entity = _entityList.FindById(idToFind) as FirstSubProcess;
            //Assert
            entity.Name.Should().Be($"{idToFind}");
        }

        [Test]
        public void Entity_Can_Be_Retrieved_By_Criteria_Entity_List()
        {
            // Arrange
            InitializeListOfFirsSubProcessEntity(10, DateTime.Now);
            var expectedId = 4;
            var nameToFind = "4";
            //Act
            var entity = _entityList.FindEntityByCriteria((ent => (ent as FirstSubProcess).Name == nameToFind)).First();
            var castedEntity = entity as FirstSubProcess;
            //Assert
            entity.Id.Should().Be(expectedId);
        }

        [Test]
        public void Entity_Can_Be_Retrieved_By_TryFindById_Entity_List()
        {
            // Arrange
            InitializeListOfFirsSubProcessEntity(10, DateTime.Now);
            var idToFind = 4;
            IEntity<int, int> entity;
            //Act
            var haveBeenFound = _entityList.TryFindById(idToFind, out entity);
            var castedEntity = entity as FirstSubProcess;
            //Assert
            haveBeenFound.Should().BeTrue();
            castedEntity.Name.Should().Be($"{idToFind}");
            entity.Id.Should().Be(idToFind);
        }

        [Test]
        public void TryFindById_Return_Null_Entity_When_Not_Found()
        {
            // Arrange
            InitializeListOfFirsSubProcessEntity(10, DateTime.Now);
            var idToFind = 14;
            IEntity<int, int> entity;
            //Act
            var haveBeenFound = _entityList.TryFindById(idToFind, out entity);
            //Assert
            haveBeenFound.Should().BeFalse();
            entity.Should().BeNull();
        }

        private void InitializeListOfFirsSubProcessEntity(int nbEntity, DateTime dateStart)
        {
            for (int i = 0; i < nbEntity; i++)
            {
                var date = dateStart;
                date.AddMinutes(i);
                _entityList.Add(new FirstSubProcess($"{i}", i, DateTime.Now));
            }
        }
    }
}
