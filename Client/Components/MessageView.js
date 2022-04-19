import React, {useState, useRef} from 'react'
import { StyleSheet,View, TextInput, Image,TouchableOpacity, ScrollView } from 'react-native'
import {Message, MessageModel } from '../Models/MessageModel';

const MessageView = () => 
{
    let _InputHolder;
    let _PrevInput;
    let input;
    let scrollview;
    //#region  hooks
    const [msg, setMsg] = useState([]); 

    const addMessage = () =>
    {
        input.clear();
        if(typeof(_InputHolder) === 'undefined'){  return;} //TODO: _InputHolder jest pusty po wysłaniu wiadomości
        else if (_InputHolder.charAt(0) === '/') {handleCommands(); return;}
        let ml = new MessageModel(_InputHolder,"User");
        setMsg([...msg, ml]);
    }

    const setMessage = (m) =>
    {
        if(typeof(m) === 'undefined'){ return;}
        _PrevInput = _InputHolder;
        _InputHolder = m;
    }

    const handleCommands =() =>
    {
        switch(_InputHolder)
        {
            case "/clear":
                setMsg([]);
        }
    }
    //#endregion
    const reff = useRef();
    return (
        <View style={styles.con}>
           <View style={styles.msgContainer}>
             <ScrollView ref={reff} onContentSizeChange={() => reff.current.scrollToEnd({animated: true})}>
                   {msg.map((item, index) => {return (<Message model={item} />)})}
             </ScrollView>
           </View>
        

        <View style={styles.MessageInputView}>

        <TextInput ref={u => {input = u}} style={styles.Input} onChangeText={(value) => setMessage(value)} maxLength={255} multiline={true} placeholder="Message @"> </TextInput>
            <TouchableOpacity ref={u => {scrollview = u}} onContentSizeChange={() => scrollview.scrollToEnd({animated: true})} onPress={addMessage}>
             <Image style={styles.SendMessage} source={require('../assets/tempsend.png')}>
             </Image>
            </TouchableOpacity>
        </View>
        </View>

    );
}
const styles = StyleSheet.create({ 
    con:
    {
        flex:1,
        flexDirection:'column'
    },
    MessageInputView:{    //Nie moze byc position: 'absolute' bo <TouchableOpacity> nie dziala ;C  
        borderRadius: 12,
        flexDirection:'row',
        alignItems: 'flex-end',
        paddingHorizontal: 10,
    },
    SendMessage:{
        bottom:6,
        width:50,
        height:50
    },
    messages:{
        flex:12,
        flexDirection: 'column',
        justifyContent: 'space-between'
    },
    Input:{
        paddingVertical:15,
        paddingHorizontal:15,
        bottom:10,
        width: 300,
        height:50,
        borderRadius:50,
        borderColor:'black',
        borderWidth:0.5
    },
    msgContainer:{
        marginTop: 30,
        marginBottom:10,
        flex:2
    }
})

export default MessageView;