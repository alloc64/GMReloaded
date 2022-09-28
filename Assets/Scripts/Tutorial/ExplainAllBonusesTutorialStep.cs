/************************************************************************
 * Copyright (c) 2014 Milan Jaitner                                     *
 * This program is free software: you can redistribute it and/or modify *
 * it under the terms of the GNU General Public License as published by *
 * the Free Software Foundation, either version 3 of the License, or    * 
 * any later version.													*
																		*
 * This program is distributed in the hope that it will be useful,      *
 * but WITHOUT ANY WARRANTY; without even the implied warranty of       *
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the         *
 * GNU General Public License for more details.							*
																		*
 * You should have received a copy of the GNU General Public License	*
 * along with this program.  If not, see http://www.gnu.org/licenses/	*
 ***********************************************************************/

using UnityEngine;
using System.Collections.Generic;
using GMReloaded;
using System;
using CodeStage.AntiCheat.ObscuredTypes;
using GMReloaded.Bonuses;
using GMReloaded.Achievements;
using GMReloaded.Tutorial;

namespace GMReloaded
{
	public class ExplainAllBonusesTutorialStep : TutorialStep
	{ 
		public ExplainAllBonusesTutorialStep(TutorialEvent tutorialEvent) : base(tutorialEvent)
		{
			Entities.EntityContainer powerupSpawnEntityContainer = null;

			OnStepStarted += () =>
			{
				DemolishAllEntities();

				powerupSpawnEntityContainer = sceneObjects.GetObject<Entities.EntityContainer>("box_default_PowerupSpawner_Holy");

				fadeInOut.FadeInOut(() => 
				{
					lcre.ForceWeaponAtSlotIdx(0, WeaponType.Knife);
					SpawnOnDefaultSpawn();
				}); 	
			};

			SetOnStartPlayerLimitations(RobotEmilViewObserver.PlayerLimitations.All);

			AddMessage("Tut_Powerups_1");

			AddMessage("Tut_Powerups_2");
			AddMessage("Tut_Powerups_2_Passive").OnMessageStarted += () => powerupSpawnEntityContainer.GenerateBonus_Tutorial(Bonus.Behaviour.GrenadePlus);
			AddMessage("Tut_Powerups_2_Passive_1");

			AddMessage("Tut_Powerups_2_Active").OnMessageStarted += () => powerupSpawnEntityContainer.GenerateBonus_Tutorial(Bonus.Behaviour.Detonation);
			AddMessage("Tut_Powerups_2_Active_1", TutorialStep.GetPositiveKey(Config.Player.KeyBind.bonusKeys[0]), TutorialStep.GetPositiveKey(Config.Player.KeyBind.bonusKeys[1]), TutorialStep.GetPositiveKey(Config.Player.KeyBind.bonusKeys[2]));

			AddMessage("Tut_Powerups_2_Rare").OnMessageStarted += () => powerupSpawnEntityContainer.GenerateBonus_Tutorial(Bonus.Behaviour.GunUpgrade);
			AddMessage("Tut_Powerups_2_Rare_1");

			AddMessage("Tut_Powerups_2_Debuff").OnMessageStarted += () => powerupSpawnEntityContainer.GenerateBonus_Tutorial(Bonus.Behaviour.Magnet);
			AddMessage("Tut_Powerups_2_Debuff_1");
		
			AddMessage("Tut_Powerups_2_Heal").OnMessageStarted += () => powerupSpawnEntityContainer.GenerateBonus_Tutorial(Bonus.Behaviour.HPPack);

			AddMessage("Tut_Powerups_2_Others").OnMessageStarted += () => powerupSpawnEntityContainer.GenerateBonus_Tutorial(Bonus.Behaviour.PointsPack);

			AddMessage("Tut_Powerups_3").OnMessageStarted += () => powerupSpawnEntityContainer.GenerateBonus_Tutorial(Bonus.Behaviour.GrenadePlus);

			SetDelay(2f);
		}
	}
}