using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Quest.Tests.Runtime
{
    public sealed class QuestCatalogTests
    {
        [Test]
        public void ReplaceQuests_WhenCalled_ReplacesCatalogContents()
        {
            QuestCatalog catalog = ScriptableObject.CreateInstance<QuestCatalog>();

            try
            {
                QuestDefinition quest = new QuestDefinition("quest_001", "Quest", "설명", null, null, null, false);
                catalog.ReplaceQuests(new[] { quest });

                Assert.AreEqual(1, catalog.Count);
                Assert.AreSame(quest, catalog.Quests[0]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void ReplaceQuests_WhenSourceChanges_KeepsOwnSnapshot()
        {
            QuestCatalog catalog = ScriptableObject.CreateInstance<QuestCatalog>();

            try
            {
                List<QuestDefinition> source = new List<QuestDefinition>
                {
                    new QuestDefinition("quest_001", "Quest", "설명", null, null, null, false)
                };

                catalog.ReplaceQuests(source);
                source.Clear();

                Assert.AreEqual(1, catalog.Count);
                Assert.AreEqual(1, catalog.Quests.Count);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void ReplaceQuests_WhenSourceIsNull_ThrowsArgumentNullException()
        {
            QuestCatalog catalog = ScriptableObject.CreateInstance<QuestCatalog>();

            try
            {
                Assert.Throws<ArgumentNullException>(() => catalog.ReplaceQuests(null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(catalog);
            }
        }
    }
}