import React from 'react'
import { StyleSheet,View, TextInput, Image,TouchableOpacity } from 'react-native'
import MessageView from './MessageView';

export default class MessageInput extends React.Component
{
    constructor(props)
    {
        super(props);
        this.state =
        {
            message: '',
        };
        this.AddMessage = this.AddMessage.bind(this);
        this.setMessage = this.setMessage.bind(this);    
    }

    AddMessage() //Debug
    {
        console.log('d');
        MessageView.Push("hi");
    }

    setMessage(value)
    {
        this.setState({message: value});
    }
 
    render() {
        return (
            <View style={styles.MessageInputView}>
                  <TextInput style={styles.Input} onChangeText={(value) => this.setMessage(value)} maxLength={255} multiline={true} placeholder="Message @"> </TextInput>
                <TouchableOpacity onPress={this.AddMessage}>
                 <Image style={styles.SendMessage} source={require('../assets/tempsend.png')}>
                 </Image>
                </TouchableOpacity>
               
            </View>
        );
      } 
}
const styles = StyleSheet.create({
    MessageInputView:{    //Nie moze byc position: 'absolute' bo <TouchableOpacity> nie dziala ;C
        flex: 1,
        flexDirection: 'row',
        alignItems: 'flex-end',
      
    },
    SendMessage:{
      width: 50,
      height: 50,
      justifyContent: 'flex-end'
    },
    Input:{
        left: 50,
        width: 300
    }

})