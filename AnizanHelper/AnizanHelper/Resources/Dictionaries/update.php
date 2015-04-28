<?php

$dictionaryFile = "dictionary.txt";
$versionFile = "version.txt";
$updatesFile = "updates.txt";

$option = isset($_GET["t"]) ? $_GET["t"] : "";

if($option == "v"){
	$str = file_get_contents($versionFile);
	print($str);
}
else if($option == "u"){
	$str = file_get_contents($updatesFile);
	print($str);
}
else{
	$str = file_get_contents($dictionaryFile);
	print($str);
}

exit();