#
# Check the NFK Planet for new players and send a message on IRC channel 
#  if pcount == maxpcount - 1
#
# (c) HarpyWar (harpywar@gmail.com)
#

$programfile = "D:\NFK\IRCBot\bin\irccdctl.exe"
$channelname = "#nfk"
$servername = "wenet"

$wc = New-Object system.Net.WebClient;
$json = $wc.downloadString("http://nfk.pro2d.ru/api.php?action=gsl")

$items = $json | ConvertFrom-Json

foreach ($item in $items)
{
	$player_count = 0
	$bot_count = 0
	
	$players = @()
	foreach ($p in $item.players) {
		$players += $p.name
		if ($p.nick) {
			$player_count ++
		} else {
			$bot_count ++
		}
	}
	$players = $players -join ', '
	
	$gametype = $item.gametype
	$map = $item.map
	$load = $item.load
	
	$pcount = $item.load[0]
	$pmax = $item.load[2]
	
	$waiting = $FALSE
	# if players == 2 in DM mode
	if ($gametype -eq "DM" -and $pmax -eq 2) { $waiting = $TRUE }
	# if gametype == TDM|CTF|DOM && players == 2
	if (($gametype -eq "TDM" -or $gametype -eq "CTF" -or $gametype -eq "DOM") -and $pcount -eq $pmax-1) { $waiting = $TRUE }
	
	if ($waiting)
	{
		$verb = "are"
		if ($player_count -eq 1) { $verb = "is" }
		$message = '"' + "$players $verb waiting for YOU on the Planet: ($load) [$gametype] $map" + '"'
	
		start-process "$programfile" -args "message $servername $channelname $message"
	}
	#"it works"
}