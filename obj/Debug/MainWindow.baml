   M S B A M L   `   `   `  ����R  NCodingChallengeV2Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA%clr-namespace:CodingChallengeV2ClientCodingChallengeV2Client  5   	   (    "CodingChallengeV2Client.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   7local%clr-namespace:CodingChallengeV2Client     5   	     � Title$  Coding Challenge��5   	   $	��480��6"   $	��480��6/   $	��655��6?   $	��655��6K    � WindowStartupLocation$ CenterScreen=�6Z    � 	Resources 5	       �� 
TargetType �� Property �� Value��%�  '��       '�� �     '�� 1    �� 5
      " ��6   .�5      ��  � ��$ 4q�6!   �� 5       � ��$
 Left=�6.   �� 5       � ��$ 23��6!   �� 5       �@��TextWrapping$
 Wrap=�6'   5      �� 5      " ��6   .�5      ��  � ��$ 4q�6!   �� 5       � ��$
 Left=�6.   5      �� 5      " ��6   .�5      ��  � ��$ Center=�6,   5      5      .��5      � $��8q�6	    � ColumnDefinitions 5      �� 5      $
��Auto �6   �� 5       � RowDefinitions 5      �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5       $
��Auto �6   �� 5!      $
��Auto �6   �� 5"      $
��Auto �6   �� 5#      $
��Auto �6   �� 5$      $
��Auto �6   �� 5%      $��* �6   .+�5'      �� $��Test data size:��6   �� 5(      $��1��6    �� Orientation$ 
Horizontal=�6!   .+�5)      �� -   5        	 ��Name$	 txtTestDataSize��5)      $��250000��6)   $��62��67   �� 5*      $��(bytes)��6   5+      �� 5,      -   5        
 ��Name$
 btnGenerateNewTestData��5,   *   $��1��6   $��1��6   $&�� Reset and generate new test data��6J   $	��227��6u   $��4,4,4,32q�6�   �� 5-      -   5         ��Name$ lblHostname��5-      $��2��6   $��	Hostname:��6.   �� 5.      -   5        $	 txtServerHostname��5.   +   $��2��6   $��1��6   $%��codingchallenge.identityone.net��6F   $	��379��6m   �� 5/      -   5        $ lblPort��5/      $��3��6   $��Port:��6*   �� 50      -   5        $	 txtServerPort��50   +   $��3��6   $��1��6   $
��4544��6B   $��62��6N   �� 51      $��4��6   $��Timeout:��6   �� 52      $��4��6   $��1��6   $ 
Horizontal=�6.   .+�53      �� -   5        $	 
txtTimeout��53      $
��3000��6$   $��62��60   �� 54      $
��(ms)��6   55      �� 56      $��5��6   $��1��6   $ 
Horizontal=�6.   $��0,0,0,8q�6G   .+�57      �� -   5        $
 btnEncodeData��57      $��Encode Data��6&   $	��152��6<   6H   �� 58      -	   5        $
 btnDecodeData��58      $��Decode Data��6&   $	��152��6<   6H   59      �� 5:      $��6��6   $��Encode/Decode:��6   �� 5;      -
   5        $ lblEncodeCycleCount��5;   )   $��6��6   $��1��6   $��0��6F   ]� 5<      $��7��6   $��2��6   $��0,16,0,0q�61   .+�5=      �� $��Status:��6   $	��Top=�6    �� 5>      -   5         ��Name$ 	lstStatus��5>      $��4,0,4,4q�6$   5?      5@      5A      