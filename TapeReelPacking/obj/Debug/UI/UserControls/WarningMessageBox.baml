   M S B A M L   `   `   `  ����J  FTapeReelPacking, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullQ-clr-namespace:TapeReelPacking.UI.UserControlsTapeReelPacking.UI.UserControls  5      7    1TapeReelPacking.UI.UserControls.WarningMessageBox   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ?local-clr-namespace:TapeReelPacking.UI.UserControls     5        � 	Resources  5
         continueImage � RowDefinitions �� TextAlignment  
AbortImage  	NextImage  PreviousImage  
RetryImage��%�  &	        &	 &    &	 9    &	 J    &	 _    � 5   
    5      �� 5      $��3* �6    6.   �� 5      $��1* �6    6.   .+�5      �� $��0��6   $+��%/Resources/ContinueSequenceButton.png��6#   $��3q�6R   6_   �� 5      $��1��6   $��Continue��6%   
���   �$ Center=�6H   $��20(�6_   6o   5      � 5   
    5      �� 5      $��3* �6    6.   �� 5      $��1* �6    6.   .+�5      �� $��0��6   $(��"/Resources/AbortSequenceButton.png��6#   6Q   �� 5      $��1��6   $��Abort��6%   
���   �$ Center=�6E   $��20(�6\   6l   5      � 5   
    5      �� 5      $��3* �6    6.   �� 5       $��1* �6    6.   .+�5"      �� $��0��6   $'��!/Resources/SequenceNextButton.png��6#   6P   �� 5#      $��1��6   $
��Next��6%   
���   �$ Center=�6D   $��20(�6[   6k   5$      � 5&   
    5'      �� 5(      $��3* �6    6.   �� 5)      $��1* �6    6.   .+�5+      �� $��0��6   $+��%/Resources/SequencePreviousButton.png��6#   6T   �� 5,      $��1��6   $
��Back��6%   
���   �$ Center=�6D   $��20(�6[   6k   5-      � 5/   
    50      �� 51      $��3* �6    6.   �� 52      $��1* �6    6.   .+�54      �� $��0��6   $"��/Resources/SequenceRetry.png��6#   6K   �� 55      $��1��6   $��Retry��6%   
���   �$ Center=�6E   $��20(�6\   6l   56      5:      .��5<      � 
������� 5=   
   �� 5>      $
��0.5* �6   �� 5?      $��2* �6   �� 5@      $
��1.5* �6   .+�5B   
   �� $��0��6   
��� ���
����� �$��1q�6I   ._�5C      �� $��Warning Message��6   $��Center=�6.   $��Center=�6K   $��Center=�6f   
���  ��$��20(�6�   6�   5D      �� 5E   
   -   5         ��Name$ txtWarningMessage��5E   ,   $��5q�6   $��1��6   $	��...��6G   $
��Wrap=�6R   $��Center=�6f   $��Center=�6�   $ Center=�6�   $��20(�6�   
���   �6�   � 5F   
   $��2��6    � ColumnDefinitions 5G      �� 5H      $��2* �6#   �� 5I      .+�5K      �� $��1��6    �� Orientation$ 
Horizontal=�6*   $��Right=�6C   $��10,5,5,5q�6_   .+�5L      �� -   5         ��Name$ btn_Sequence_Continue��5L      
������ $��0q�6R   #��[  5M      $��5,0,5,0q�6d   6w   �� 5N      -   5        $ btn_Sequence_Abort��5N      
������ $��0q�6N   #��[ 5O      $��5,0,5,0q�6\   6o   5P      �� 5R      $��0��6   $ 
Horizontal=�6*   $��Right=�6C   $��10,5,5,5q�6_   .+�5S      �� -   5        $ btn_Sequence_Previous��5S      
������ $��0q�6Q   #��[ 5T      $��5,0,5,0q�6c   6v   �� 5V      -   5        $ btn_Sequence_Next��5V      
������ $��0q�6N   #��[ 5W      $��5,0,5,0q�6^   6q   �� 5Y      -   5        $ btn_Retry_Current_Step��5Y      
������ $��0q�6S   #��[ 5Z      $��5,0,5,0q�6d   6w   5\      5]      5a      5b      