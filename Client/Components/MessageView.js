import React from 'react';
import { StyleSheet, Text, ScrollView, SafeAreaView,Flatlist } from 'react-native';

export default class MessageView extends React.Component
{
    static viewComponent = []; 

    constructor(props)
    {
        super(props);
        this.state = 
        {
            Messages : []
        };

        for(let i =0;i<50;i++)
        { 
            MessageView.Push(i)
        }
    }

    static Push(obj)
    {
        
        MessageView.viewComponent.push({obj});
    }
    MessageRenderer()
    {
      
    }
    
    render(){
        return(
            <SafeAreaView style={styles.messages}>
             <ScrollView>
                 {  MessageView.viewComponent.map((i) => {} )}
             </ScrollView>
            </SafeAreaView>
        )
    }
}

const styles = StyleSheet.create({
    messages:{
        flex:12,
        flexDirection: 'column',
        justifyContent: 'space-between'
    },
    text:{
        fontSize: 50,
    }
})