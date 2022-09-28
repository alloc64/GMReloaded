
using UnityEditor;
using System.Collections;

namespace TouchOrchestra
{
	public class CheatOptions 
	{
		[MenuItem("TouchOrchestra/Grenade Madness/Set Level 20")]
		private static void SetLevel20()
		{
			GMReloaded.LocalClientRobotEmil.levelPoints = GMReloaded.Config.Exp.GetLevelDesiredPoints(20);
			GMReloaded.LocalClientRobotEmil.level = 20;
		}

		[MenuItem("TouchOrchestra/Grenade Madness/Set Level 50")]
		private static void SetLevel50()
		{
			GMReloaded.LocalClientRobotEmil.levelPoints = GMReloaded.Config.Exp.GetLevelDesiredPoints(50);
			GMReloaded.LocalClientRobotEmil.level = 50;
		}

		[MenuItem("TouchOrchestra/Grenade Madness/Force tutorial completed")]
		private static void ForceTutorialCompleted()
		{
			GMReloaded.Tutorial.TutorialManager.isTutorialCompleted = true;
		}
	}

}
