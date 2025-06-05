**Change Type**: Issue</br>
**Required**: Yes</br>
**Description**: </br>
This patch fixes an issue where reel spin events are triggered on the first reel that causes the state change (eg from spinning to overshooting). At that point the other reels in a reel group haven't changed state yet and will not have populated the final symbols.</br>
</br>

**Additional Information:**</br>
