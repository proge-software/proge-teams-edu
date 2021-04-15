#######
### please read the docs: https://docs.microsoft.com/en-us/graph/cloud-communication-online-meeting-application-access-policy
#######

Import-Module MicrosoftTeams

$userCredential = Get-Credential
$sfbSession = New-CsOnlineSession -Credential $userCredential
Import-PSSession $sfbSession

#ID dell'application registration (service account) che chiama Graph API
$applicationRegistrationID = Read-Host -Prompt "Insert App Registration IDs"
$policyName = "ProgeEdu-GetOnlineMeeting-Policy"

# da eseguire una tantum
New-CsApplicationAccessPolicy -Identity $policyName -AppIds $applicationRegistrationID -Description "access policy for get online meetings"

#ESEMPI di aggiornamento dell'access policy

# l'utente deve avere la licenza assegnata
# ogni utente può avere una access policy assegnata
# Grant-CsApplicationAccessPolicy -PolicyName $policyName -Identity mario.rossi@esempio.edu

# assegnazione una tantum a tutti gli utenti
Get-CsOnlineUser | Grant-CsApplicationAccessPolicy -PolicyName $policyName

# assegnazione basata su una proprietà dell'utente tutti gli utenti del dominio esempio.edu che non hanno la policy specificata
# Get-CsOnlineUser | Where-Object {  $_.userprincipalname -like '*@esempio.edu' -ne $policyName}