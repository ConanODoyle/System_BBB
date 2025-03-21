//for now a basic implementation that doesn't cover everything


if(isObject(WinCondition_Innocent))
{
	WinCondition_Innocent.delete();
}
new ScriptObject(WinCondition_Innocent)
{
	superClass = WinCondition_Basic;
};

if(isObject(WinCondition_Traitor))
{
	WinCondition_Traitor.delete();
}
new ScriptObject(WinCondition_Traitor)
{
	superClass = WinCondition_Basic;
};

function WinCondition_new(%name)
{
	if(isObject(%name))
	{
		%name.delete();
	}

	new ScriptObject(%name)
	{
		superClass = WinCondition_Basic;
	};

}

function WinCondition_set(%player,%condition)
{
	if(%condition.superClass !$= "WinCondition_Basic")
	{
		warn("Non existant win condition");
		backtrace();
		return;
	}

	%player.winCondition = %condition.getId();
}

// Traitors miskill Traitors
// Innocents and Detectives miskill Innocents and Detectives
function WinCondition_Basic::isMiskill(%obj,%target)
{

	return %obj == %target.client.winCondition;
}

// Detectives, Innocents, and Traitors can validly kill anyone with hard validation
// Detectives and Innocents can never validly kill detectives
$KillType::Valid = 0;
$KillType::Invalid = 1;
$KillType::CriminalInvalid = 2;
$KillType::Uknown = 3;
function WinCondition_Basic::getKillType(%obj,%player,%target)
{
	//bypass for innos killing traitors correctly without a callout
	if (!%obj.isMisKill(%target))
	{
		return $KillType::Valid;
	}

	//invalid
	if(%target.isValidState(%player,$ValidState::Invalid))
	{
		return $KillType::Invalid;
	}

	//valid
	if(%target.isValidState(%player,$ValidState::Criminal) || %target.isValidState(%player,$ValidState::CriminalCallout))
	{
		return $KillType::Valid;
	}

	//was a miskill
	if(%obj.isMisKill(%target))
	{
		//the player who was soonest to be criminal loses an oopsie
		%soonestCriminal = %player.getSoonestCriminal(%target);
		if( %soonestCriminal !$= "")
		{
			return $KillType::CriminalInvalid;
		}
		return $KillType::Valid;
	}
	
	//otherwise both players lose an oopsie
	//this is mostly to handle cases were it's hard to determine player intentions
	//the target can either refund both of the oopsies or make a vote to only refund their oopsie
	//hopefully this doesn't happen too often
	return $KillType::Uknown;
}

WinCondition_new("WinCondition_Innocent");

WinCondition_new("WinCondition_Traitor");
// Traitors can invalidly kill innocents and detectives
function WinCondition_Traitor::getKillType(%obj,%player,%target)
{
	// invalid
	if(%obj.isMisKill(%target) && %player.isValidState(%target,$ValidState::Invalid))
	{
		return $KillType::Invalid;
	}

	// was a miskill
	if(%obj.isMisKill(%target))
	{
		//the player who was soonest to be criminal loses an oopsie
		%soonestCriminal = %player.getSoonestCriminal(%target);
		if( %soonestCriminal !$= "")
		{
			return $KillType::Invalid;
		}
		return $KillType::Valid;
	}
	
	// all other kills are valid for traitors
	return $KillType::Valid;
}