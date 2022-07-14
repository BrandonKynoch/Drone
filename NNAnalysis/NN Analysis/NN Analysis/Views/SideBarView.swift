//
//  SideBarView.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 7/13/22.
//

import SwiftUI

struct SideBarView: View {
    @EnvironmentObject var dataHandler: DataHandler
    
    var body: some View {
        ZStack {
            BackgroundView(viewMode: .light)
            
            if dataHandler.openTrainingFolders.count == 0 {
                FolderSelectionView()
            } else {
                VStack {
                    Rectangle().foregroundColor(.black)
                        .overlay(Text("Penis").modifier(TextModifier(size: 30, weight: .bold)))
                    
                    Spacer()
                    
                    SelectFolderButton()
                    
                    Spacer().frame(height: 50)
                }
            }
        }
    }
}

struct SideBarView_Previews: PreviewProvider {
    static var previews: some View {
        SideBarView()
    }
}
